// <copyright file="EventController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Teams.Apps.Celebration.Authentication;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Event;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Occurrence;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.User;
    using Microsoft.Teams.Apps.Celebration.Models;

    /// <summary>
    /// Controller for the event data.
    /// </summary>
    [Route("api/events")]
    [Authorize]
    public class EventController : CelebrationControllerBase
    {
        private readonly EventRepository eventRepository;
        private readonly OccurrenceRepository occurrenceRepository;
        private readonly UserRepository userRepository;
        private readonly UserValidator userValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventController"/> class.
        /// </summary>
        /// <param name="eventRepository">Event repository instance.</param>
        /// <param name="occurrenceRepository">Occurrence repository service.</param>
        /// <param name="userRepository">User repository service.</param>
        /// <param name="userValidator">User validation service. It checks if a user is authorized to run a method of the controller.</param>
        public EventController(
            EventRepository eventRepository,
            OccurrenceRepository occurrenceRepository,
            UserRepository userRepository,
            UserValidator userValidator)
        {
            this.eventRepository = eventRepository;
            this.occurrenceRepository = occurrenceRepository;
            this.userRepository = userRepository;
            this.userValidator = userValidator;
        }

        /// <summary>
        /// Gets time zone data list.
        /// </summary>
        /// <returns>The requested timezone data list.</returns>
        [HttpGet("timezones")]
        public async Task<ActionResult<IEnumerable<EventTimeZone>>> GetTimeZones()
        {
            var result = System.TimeZoneInfo.GetSystemTimeZones()
                .Select(p => new EventTimeZone
                {
                    Id = p.Id,
                    Timezone = p.DisplayName,
                });

            return this.Ok(await Task.FromResult(result));
        }

        /// <summary>
        /// Get all events.
        /// </summary>
        /// <returns>A list of <see cref="Event"/> instances.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetAllEventsAsync()
        {
            var entities = await this.eventRepository.GetAllEventsAsync(this.UserAadId);
            return this.Ok(entities.Select(p =>
            {
                int.TryParse(p.Image, out int imageCode);
                return new Event
                {
                    Id = p.Id,
                    EventType = p.EventType,
                    Date = p.Date,
                    TimeZone = p.TimeZone,
                    Image = imageCode,
                    Message = p.Message,
                    Title = p.Title,
                    SharedTeams = p.SharedTeams,
                };
            }));
        }

        /// <summary>
        /// Get an event by Id.
        /// </summary>
        /// <param name="id">Event Id.</param>
        /// <returns>It returns the event with the passed in id.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEventByIdAsync(string id)
        {
            var entity = await this.eventRepository.GetAsync(id);
            if (entity == null)
            {
                return this.NotFound();
            }

            int.TryParse(entity.Image, out int imageCode);
            var result = new Event
            {
                Id = entity.Id,
                EventType = entity.EventType,
                Title = entity.Title,
                Image = imageCode,
                Message = entity.Message,
                Date = entity.Date,
                TimeZone = entity.TimeZone,
                SharedTeams = entity.SharedTeams,
            };

            return this.Ok(result);
        }

        /// <summary>
        /// Gets events by data range for a team.
        /// </summary>
        /// <param name="fromDateTime">The from date of the date range.</param>
        /// <param name="toDateTime">The to date of the date range.</param>
        /// <param name="teamId">The team id.</param>
        /// <returns>The event data list.</returns>
        [HttpGet("from/{fromDateTime}/to/{toDateTime}/team/{teamId}")]
        public async Task<ActionResult<IEnumerable<EventSummary>>> GetEventsByDateRangeAndTeamIdAsync(
            DateTime fromDateTime,
            DateTime toDateTime,
            string teamId)
        {
            var isUserValid = await this.userValidator.ValidateAsync(this.UserTenantId, teamId, this.UserAadId);
            if (!isUserValid)
            {
                return this.Forbid();
            }

            if (fromDateTime == DateTime.MinValue)
            {
                throw new ArgumentException("Invalid fromDateTime.");
            }

            if (toDateTime == DateTime.MinValue)
            {
                throw new ArgumentException("Invalid toDateTime.");
            }

            if (toDateTime < fromDateTime)
            {
                throw new ArgumentException("toDateTime should be larger or equal to fromDateTime.");
            }

            if (toDateTime - fromDateTime > TimeSpan.FromDays(365))
            {
                throw new ArgumentException("Date range should be less than 365days.");
            }

            var entities = await this.eventRepository.GetEventsByDateRangeAndTeamIdAsync(
                fromDateTime,
                toDateTime,
                teamId);
            var result = entities.Select(p =>
                new EventSummary
                {
                    Id = p.Id,
                    EventType = p.EventType,
                    Date =
                        p.Date.Value.Month >= fromDateTime.Month && p.Date.Value.Day >= fromDateTime.Day
                        ? $"{fromDateTime.Year}-{p.Date.Value.Month}-{p.Date.Value.Day}"
                        : $"{toDateTime.Year}-{p.Date.Value.Month}-{p.Date.Value.Day}",
                    Title = p.Title,
                    Message = p.Message,
                    Owner = p.OwnerName,
                })
                .OrderBy(p => p.Date);
            return this.Ok(result);
        }

        /// <summary>
        /// Delete an existing event.
        /// </summary>
        /// <param name="id">The id of the event to be deleted.</param>
        /// <returns>If the passed in Id is invalid, it returns 404 not found error. Otherwise, it returns 200 Ok.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEventAsync(string id)
        {
            var entity = await this.eventRepository.GetAsync(id);
            if (entity == null)
            {
                return this.NotFound();
            }

            await this.eventRepository.DeleteAsync(entity);
            return this.Ok();
        }

        /// <summary>
        /// Create a new event.
        /// </summary>
        /// <param name="celebrationEvent">Celebration event to be created.</param>
        /// <returns>The newly created event.</returns>
        [HttpPost]
        public async Task<ActionResult<Event>> CreateEventAsync([FromBody]Event celebrationEvent)
        {
            var userTeamsId = await this.GetUserTeamsIdAsync();

            var newId = await this.eventRepository.CreateEventAsync(
                celebrationEvent.EventType,
                celebrationEvent.Title,
                celebrationEvent.Image.ToString(),
                celebrationEvent.Message,
                celebrationEvent.Date,
                celebrationEvent.TimeZone,
                celebrationEvent.SharedTeams,
                this.UserAadId,
                this.UserName,
                userTeamsId);

            celebrationEvent.Id = newId;

            return this.Ok(celebrationEvent);
        }

        /// <summary>
        /// Update an existing event.
        /// </summary>
        /// <param name="celebrationEvent">An existing event to be updated.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateEventAsync([FromBody]Event celebrationEvent)
        {
            var existing = await this.eventRepository.GetAsync(celebrationEvent.Id);
            if (existing != null && existing.Date != celebrationEvent.Date)
            {
                await this.occurrenceRepository.DeleteInitialOccurrencesByEventIdAsync(celebrationEvent.Id);
            }

            var userTeamsId = await this.GetUserTeamsIdAsync();

            await this.eventRepository.UpdateEventAsync(
                celebrationEvent.Id,
                celebrationEvent.EventType,
                celebrationEvent.Title,
                celebrationEvent.Image.ToString(),
                celebrationEvent.Message,
                celebrationEvent.Date,
                celebrationEvent.TimeZone,
                celebrationEvent.SharedTeams,
                this.UserAadId,
                this.UserName,
                userTeamsId);

            return this.Ok();
        }

        private async Task<string> GetUserTeamsIdAsync()
        {
            var userEntity = await this.userRepository.GetAsync(this.UserAadId);
            return userEntity.UserId;
        }
    }
}