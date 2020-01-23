import * as React from 'react';
import './events.scss';
import { getBaseUrl } from '../../configVariables';
import * as microsoftTeams from "@microsoft/teams-js";
import NoEvent from '../NoEvent/noEvent';
import EventCard from '../EventCard/eventCard';
import ErrorScreen from '../ErrorScreen/errorScreen';
import { getCelebrationsEvents } from '../../api/eventsApi';
import { Loader } from '@fluentui/react';

interface ITaskInfo {
    title?: string;
    height?: number;
    width?: number;
    url?: string;
    card?: string;
    fallbackUrl?: string;
    completionBotId?: string;
}

interface IEvent {
    id: string;
    title: string;
    date: string;
    image: string;
    message: string;
}

interface IEventsState {
    url: string;
    emptyEvent: boolean;
    events: IEvent[];
    errorState: boolean;
    MaxEventCountPerUser: number;
    loader: boolean;
}

class Events extends React.Component<{}, IEventsState> {
    constructor(props: {}) {
        super(props);
        this.state = {
            url: getBaseUrl() + "/Home/editEvent",
            emptyEvent: true,
            events: [],
            MaxEventCountPerUser: 5,
            errorState: false,
            loader: true,
        }
        this.escFunction = this.escFunction.bind(this);
    }

    public componentDidMount() {
        microsoftTeams.initialize();
        document.addEventListener("keydown", this.escFunction, false);
        this.getEventsFromApi().then(() => {
            this.setState({
                loader: false,
            });

            microsoftTeams.getContext((context) => {
                if (document.referrer.includes("https://teams.microsoft.com/")) {
                    var eventId = context["subEntityId"];
                    if (eventId) {
                        var found = this.state.events.find(event => event.id === eventId);
                        if (found) {
                            this.onNewEvent(eventId);
                        }
                    }
                }
            });
        });
    }

    public componentWillUnmount() {
        document.removeEventListener("keydown", this.escFunction, false);
    }

    public escFunction(event: any) {
        if (event.keyCode === 27 || (event.key === "Escape")) {
            microsoftTeams.tasks.submitTask();
        }
    }

    public render(): JSX.Element {
        if (this.state.loader) {
            return (
                <div className="loaderContainer">
                    <Loader className="eventsLoader" />
                </div>
            );
        } else {
            if (this.state.errorState) {
                return (
                    <ErrorScreen refreshList={this.refreshEvents}></ErrorScreen>
                );
            }

            if (this.state.emptyEvent) {
                return (
                    <NoEvent newEvent={this.onNewEvent}></NoEvent>
                );
            } else {
                return (
                    <div className="eventList">
                        {this.getEventList()}
                        {this.addNewEventCard()}
                    </div>
                );
            }
        }
    }

    private getEventsFromApi = async () => {
        try {
            const response = await getCelebrationsEvents();
            this.setState({
                emptyEvent: response.data.length === 0 ? true : false,
                events: response.data,
                errorState: false,
            });
        } catch (error) {
            this.setState({
                errorState: true,
            })
            return error;
        }
    }

    public getEventList = () => {
        return this.state.events.map((event) => {
            return (
                <EventCard
                    key={event.id}
                    cardType="eventCard"
                    id={event.id}
                    title={event.title}
                    date={event.date}
                    image={event.image}
                    message={event.message}
                    newEvent={this.onNewEvent}
                    refreshList={this.refreshEvents}
                ></EventCard>
            );
        });
    }

    public addNewEventCard = () => {
        let newEvents = [];
        for (let i = 0; i < this.state.MaxEventCountPerUser - this.state.events.length; i++) {
            newEvents.push(
                <EventCard
                    key={"new event" + i}
                    cardType="newCard"
                    newEvent={this.onNewEvent}
                ></EventCard>
            );
        };
        return newEvents;
    }

    public refreshEvents = async () => {
        this.getEventsFromApi();
    }

    public onNewEvent = (id: string) => {
        let taskInfo: ITaskInfo = {
            url: this.state.url + "/" + id,
            title: "Add event",
            height: 660,
            width: 600,
            fallbackUrl: this.state.url,
        }

        let submitHandler = (err: any, result: any) => {
            this.refreshEvents();
        };

        microsoftTeams.tasks.startTask(taskInfo, submitHandler);
    }
}

export default Events;