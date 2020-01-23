import * as React from 'react';
import './editEvent.scss';
import { RouteComponentProps } from 'react-router-dom';
import { Loader, Button, Dropdown, Text } from '@fluentui/react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Input, TextArea } from 'msteams-ui-components-react';
import "react-responsive-carousel/lib/styles/carousel.min.css";
import { Carousel } from 'react-responsive-carousel';
import { DatePicker, DayOfWeek, IDatePickerStrings } from 'office-ui-fabric-react/lib/DatePicker';
import { updateEvent, getEvent, addNewCEvent, getTimezones, getTeams } from '../../api/eventsApi';
import { initializeIcons } from 'office-ui-fabric-react/lib/Icons';

type dropdownItem = {
    header: string,
    content: {
        id: string,
    },
}

enum editEventMode {
    editEventMode = 'Edit',
}

interface IeditEventState {
    editEventMode: string;
    event: any;
    id: string;
    loader: boolean;
    title: string;
    image: number;
    message: string;
    date: any;
    formattedValue?: any;
    selectedEvent: dropdownItem[],
    selectedEventType: dropdownItem,
    teams?: any[],
    selectedTeams: dropdownItem[],
    timeZone: [],
    selectedTimeZone: dropdownItem,
    timezoneList: dropdownItem[],
    selectedTimeZoneId: string,
    theme: string,
    apiError: boolean,
}

class editEvent extends React.Component<RouteComponentProps, IeditEventState> {

    constructor(props: RouteComponentProps) {
        super(props);

        this.state = {
            editEventMode: "New",
            event: {},
            id: "",
            title: "Birthday",
            image: 0,
            message: "Wishing you the best on your special day!",
            date: new Date(),
            selectedEvent: [{ header: "Other", content: { id: "1" } }, { header: "Birthday", content: { id: "2" } }, { header: "Anniversary", content: { id: "3" } }],
            selectedEventType: { header: "Birthday", content: { id: "2" } },
            selectedTeams: [],
            timeZone: [],
            selectedTimeZone: {
                header: "Select your Time Zone",
                content: {
                    id: "select"
                }
            },
            timezoneList: [],
            selectedTimeZoneId: "",
            theme: "default",
            loader: true,
            apiError: false,
        };
    }

    public componentDidMount() {
        microsoftTeams.initialize();
        initializeIcons();
        microsoftTeams.getContext((context) => {
            let theme = context.theme || "";
            this.setState({
                theme: theme
            });
        });

        const params = this.props.match.params;

        this.getTeamList().then(() => {
            this.getTimezoneList().then(() => {
                if ('id' in params) {
                    let id = params['id'];
                    this.getItem(id).then(() => {
                        const selectedTeams = this.makeDropdownItemList(this.state.selectedTeams, this.state.teams);
                        const type = this.getEventType(this.state.event.eventType);
                        this.setState({
                            id: id,
                            editEventMode: editEventMode.editEventMode,
                            title: this.state.event.title,
                            message: this.state.event.message,
                            image: parseInt(this.state.event.image),
                            date: this.state.event.date,
                            selectedTeams: selectedTeams,
                            selectedEventType: type,
                            selectedTimeZone: this.getSavedTimezone(this.state.event.timeZone),
                            selectedTimeZoneId: this.getSavedTimezone(this.state.event.timeZone).content.id,
                            loader: false,
                        })
                    });
                } else {
                    let timezone: dropdownItem = {
                        header: "Select your Time Zone",
                        content: {
                            id: "select"
                        }
                    };
                    let findTimezone = this.state.timezoneList.find(x => x.content.id === this.getLocalTimeZone());
                    if (findTimezone) {
                        timezone.content.id = findTimezone.content.id;
                        timezone.header = findTimezone.header;
                    }
                    this.setState({
                        loader: false,
                        selectedTimeZone: timezone,
                        selectedTimeZoneId: timezone.content.id,
                    });
                }
            });
        });
    }

    public render(): JSX.Element {
        let images: any = [];
        for (let i = 0; i < 11; i++) {
            images.push(
                <div className="carouselImage" key={`image-${i}`}>
                    <img src={require(`../../images/Carousel/Celebrations-bot-image-${i}-.png`)} alt="" />
                </div>
            );
        }

        const DayPickerStrings: IDatePickerStrings = {
            months: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],

            shortMonths: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],

            days: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],

            shortDays: ['S', 'M', 'T', 'W', 'T', 'F', 'S'],

            goToToday: 'Go to today',
            prevMonthAriaLabel: 'Go to previous month',
            nextMonthAriaLabel: 'Go to next month',
            prevYearAriaLabel: 'Go to previous year',
            nextYearAriaLabel: 'Go to next year',
            closeButtonAriaLabel: 'Close date picker'
        };

        const callOutProps = {
            className: "callout-" + this.state.theme,
        };

        if (this.state.loader) {
            return (
                <div className="taskmoduleLoader">
                    <Loader />
                </div>
            );
        } else {
            return (
                <div className="taskModule">
                    <div className="formContainer">
                        <Input
                            tabIndex={0}
                            className="inputField"
                            value={this.state.title}
                            label="Event title"
                            placeholder="title"
                            onChange={this.onTitleChanged}
                            autoComplete="off"
                            required
                        />
                        <Carousel infiniteLoop={true} showThumbs={false} showIndicators={false}
                            showStatus={false} useKeyboardArrows={false}
                            autoPlay={false} onChange={this.changeImage}
                            selectedItem={this.state.image}>
                            {images}
                        </Carousel>
                        <TextArea
                            className="textArea"
                            autoFocus
                            placeholder="message"
                            label="Message"
                            value={this.state.message}
                            onChange={this.onMessageChanged}
                        />
                        <div className="eventDetail">
                            <div className="eventType">
                                <h4 className="textLabel">Event type</h4>
                                <Dropdown
                                    className="eventTypeDropdown"
                                    items={this.state.selectedEvent}
                                    placeholder="Select your Event Type"
                                    value={this.state.selectedEventType.header}
                                    onSelectedChange={this.onEventTypeChange}
                                    fluid
                                />
                            </div>
                            <div className="eventDate">
                                <h4 className="textLabel">Date</h4>
                                <DatePicker
                                    className="datePicker"
                                    firstDayOfWeek={DayOfWeek.Monday}
                                    strings={DayPickerStrings}
                                    showWeekNumbers={false}
                                    firstWeekOfYear={1}
                                    showMonthPickerAsOverlay={true}
                                    placeholder="Select a date..."
                                    ariaLabel="Select a date"
                                    tabIndex={0}
                                    onSelectDate={this.onSelectDate}
                                    calloutProps={callOutProps}
                                    value={new Date(this.state.date)}
                                    formatDate={() => this.formatDate(new Date(this.state.date))}
                                />
                            </div>
                        </div>
                        <div className="eventShare">
                            <h4 className="textLabel">Share this celebration with</h4>
                            <Dropdown
                                items={this.getItems()}
                                placeholder="Share this celebration with"
                                value={this.state.selectedTeams}
                                onSelectedChange={this.onSharedTeamChange}
                                noResultsMessage="We couldn't find any matches."
                                search
                                multiple
                                fluid
                            />
                        </div>
                        <div className="postTime">
                            <h4 className="textLabel">Celebration but will post to team at</h4>
                            <span>10:00 am</span>
                            <Dropdown
                                className="timeZoneDropdown"
                                items={this.state.timezoneList}
                                placeholder="Select your Time Zone"
                                value={this.state.selectedTimeZone}
                                onSelectedChange={this.onTimezoneChange}
                                fluid
                            />
                        </div>
                    </div>

                    <div className="footerContainer">
                        <div className="buttonContainer">
                            <Loader id="savingLoader" className="hiddenLoader savingLoader" size="smallest" label="Saving event" labelPosition="end" />
                            <Text content="Sorry, an error occurred. Please try again." className="errorMessage errorHidden" error size="medium" />
                            <Button content="Cancel" id="cancelBtn" onClick={this.onCancel} secondary />
                            <Button content="Save" disabled={this.isSaveBtnDisabled()} id="saveBtn" onClick={this.onSave} primary />
                        </div>
                    </div>
                </div>
            );
        }
    }

    private getLocalTimeZone = (): string => {
        let timezone = new Date().toString();
        return /\((.*)\)/.exec(timezone)![1];
    }

    private formatDate = (date: Date): string => {
        return (date.getMonth() + 1) + '/' + date.getDate();
    };

    private getEventType = (eventType: string) => {
        let findEventType: dropdownItem = {
            header: '',
            content: {
                id: '',
            },
        };
        this.state.selectedEvent.find((item) => {
            if (item.header === eventType) {
                findEventType = item;
            }
        });
        return findEventType;
    }

    private getItem = async (id: string) => {
        try {
            const response = await getEvent(id);
            this.setState({
                event: response.data,
                selectedTeams: response.data.sharedTeams,
            });
        } catch (error) {
            return error;
        }
    }

    private getTeamList = async () => {
        try {
            const response = await getTeams();
            this.setState({
                teams: response.data
            });
        } catch (error) {
            return error;
        }
    }

    private getItems = () => {
        const resultedTeams: dropdownItem[] = [];
        if (this.state.teams) {
            const remainingUserTeams = this.state.teams;
            remainingUserTeams.forEach((element) => {
                resultedTeams.push({
                    header: element.name,
                    content: {
                        id: element.teamId
                    }
                });
            });
        }
        return resultedTeams;
    }

    private getTimezoneList = async () => {
        try {
            const response = await getTimezones();
            const dropdownItemList: dropdownItem[] = [];
            response.data.forEach((element: any) => {
                dropdownItemList.push(
                    {
                        header: element.timezone,
                        content: {
                            id: element.id,
                        }
                    }
                );
            });
            this.setState({
                timeZone: response.data,
                timezoneList: dropdownItemList,
            });
        } catch (error) {
            return error;
        }
    }

    private getSavedTimezone = (id: string): dropdownItem => {
        let timezone: dropdownItem = {
            header: "Select your Time Zone",
            content: {
                id: "select"
            }
        };
        this.state.timezoneList.forEach((item) => {
            if (item.content.id === id) {
                timezone = {
                    header: item.header,
                    content: {
                        id: item.content.id
                    }
                }
                return timezone;
            }
        });
        return timezone;
    }

    private onTitleChanged = (event: any) => {
        this.setState({
            title: event.target.value,
        });
    }

    private onMessageChanged = (event: any) => {
        this.setState({
            message: event.target.value,
        });
    }

    private changeImage = (index: number, item: React.ReactNode) => {
        this.setState({
            image: index,
        });
    }

    private onEventTypeChange = (event: any, itemsData: any) => {
        if (this.state.selectedEventType.header === itemsData.value.header) {
            this.setState({
                selectedEventType: itemsData.value,
            });
        } else {
            this.setState({
                selectedEventType: itemsData.value,
            }, () => {
                if (itemsData.value.header === "Birthday") {
                    this.setState({
                        title: "Birthday",
                        message: "Wishing you the best on your special day!",
                    });
                } else if (itemsData.value.header === "Anniversary") {
                    this.setState({
                        title: "Anniversary",
                        message: "Wishing you the best on your special day!",
                    });
                } else {
                    this.setState({
                        title: "",
                        message: "",
                    });
                }
            })
        }
    }

    private onTimezoneChange = (event: any, itemsData: any) => {
        this.setState({
            selectedTimeZone: itemsData.value,
            selectedTimeZoneId: itemsData.value.content.id,
        })
    }

    private onSharedTeamChange = (event: any, itemsData: any) => {
        this.setState({
            selectedTeams: itemsData.value,
        })
    }

    private onSelectDate = (date: Date | null | undefined) => {
        this.setState({
            date: date,
        });
    }

    private makeDropdownItemList = (items: any[], fromItems: any[] | undefined) => {
        const dropdownItemList: dropdownItem[] = [];
        items.forEach(element =>
            dropdownItemList.push(
                typeof element !== "string" ? element : {
                    header: fromItems!.find(x => x.teamId === element).name,
                    content: {
                        id: element
                    }
                })
        );
        return dropdownItemList;
    }

    private isSaveBtnDisabled = () => {
        if (this.state.title) {
            return false;
        } else {
            return true;
        }
    }

    private updateEvent = async (newEvent: {}) => {
        try {
            const response = await updateEvent(newEvent);
            this.setState({
                apiError: false,
            });
        } catch (error) {
            this.setState({
                apiError: true,
            });
            return error;
        }
    }

    private addNewCEvent = async (newEvent: {}) => {
        try {
            const response = await addNewCEvent(newEvent);
            this.setState({
                apiError: false,
            });
        } catch (error) {
            this.setState({
                apiError: true,
            });
            return error;
        }
    }

    private onSave = () => {
        let spanner = document.getElementsByClassName("savingLoader");
        spanner[0].classList.remove("hiddenLoader");
        let errorState = document.getElementsByClassName("errorMessage");
        errorState[0].classList.add("errorHidden");
        let selectedTeams: string[] = [];
        this.state.selectedTeams.forEach((element) => {
            selectedTeams.push(element.content.id);
        });
        if (this.state.editEventMode === editEventMode.editEventMode) {
            let newEvent = {
                id: this.state.id,
                title: this.state.title,
                date: this.state.date,
                image: this.state.image,
                message: this.state.message,
                timeZone: this.state.selectedTimeZoneId,
                sharedTeams: selectedTeams,
                eventType: this.state.selectedEventType.header,
            }

            this.updateEvent(newEvent).then(() => {
                if (this.state.apiError) {
                    let spanner = document.getElementsByClassName("savingLoader");
                    spanner[0].classList.add("hiddenLoader");
                    let errorState = document.getElementsByClassName("errorMessage");
                    errorState[0].classList.remove("errorHidden");
                } else {
                    microsoftTeams.tasks.submitTask();
                }
            });
        } else {
            let newEvent = {
                title: this.state.title,
                date: this.state.date,
                image: this.state.image,
                message: this.state.message,
                timeZone: this.state.selectedTimeZoneId,
                sharedTeams: selectedTeams,
                eventType: this.state.selectedEventType.header,
            }
            this.addNewCEvent(newEvent).then(() => {
                if (this.state.apiError) {
                    let spanner = document.getElementsByClassName("savingLoader");
                    spanner[0].classList.add("hiddenLoader");
                    let errorState = document.getElementsByClassName("errorMessage");
                    errorState[0].classList.remove("errorHidden");
                } else {
                    microsoftTeams.tasks.submitTask();
                }
            });
        }
    }

    private onCancel(event: any) {
        microsoftTeams.tasks.submitTask();
    }
}

export default editEvent;