import * as React from 'react';
import './teamEvents.scss';
import * as microsoftTeams from "@microsoft/teams-js";
import { getTeamEvents, getChangeMessageTargetCard, saveMessageTargetInfo, getMessageTarget } from '../../api/eventsApi';
import { Loader, Accordion, Table, Avatar, Text, Button } from '@fluentui/react';
import { RouteComponentProps } from 'react-router-dom';
import { TaskInfo } from '@microsoft/teams-js';

interface IEvent {
    id: string;
    title: string;
    date: string;
    type: string;
    message: string;
    owner: string;
}

interface ITeamEventsState {
    teamId: string;
    currentMonthStart: string;
    currentMonthEnd: string;
    nextMonthStart: string;
    nextMonthEnd: string;
    emptyEvent: boolean;
    currentMonthEvents: IEvent[];
    lastMonthEvents: IEvent[];
    errorState: boolean;
    loader: boolean;
    messageTargetChannel: string;
}

class TeamEvents extends React.Component<RouteComponentProps, ITeamEventsState> {
    constructor(props: RouteComponentProps) {
        super(props);
        let param = this.props.location.search;
        let teamId = "";
        if (param.includes('teamId=')) {
            teamId = param.slice(param.indexOf("=") + 1);
        } else {
            console.log("no teamId");
        }

        let date = new Date();
        let fullYear = date.getFullYear();
        let month = date.getMonth();
        this.state = {
            teamId: teamId,
            currentMonthStart: new Date(fullYear, month, 1).toISOString(),
            currentMonthEnd: new Date(fullYear, month + 1, 0).toISOString(),
            nextMonthStart: new Date(fullYear, month + 1, 1).toISOString(),
            nextMonthEnd: new Date(fullYear, month + 2, 0).toISOString(),
            emptyEvent: true,
            currentMonthEvents: [],
            lastMonthEvents: [],
            errorState: false,
            loader: true,
            messageTargetChannel: '',
        }
    }

    public componentDidMount() {
        this.getEventsFromApi().then(() => {
            this.setState({
                loader: false,
            });
        });
    }

    public render(): JSX.Element {
        let currentMonthEvents: any = [];
        let lastMonthEvents: any = [];
        this.renderRow(this.state.currentMonthEvents, currentMonthEvents);
        this.renderRow(this.state.lastMonthEvents, lastMonthEvents);
        const panels = [
            {
                title: 'Current Month Celebrations',
                content: {
                    key: 'current',
                    content: this.getRenderRow(this.state.currentMonthEvents, currentMonthEvents),
                },
            },
            {
                title: 'Next Month Celebrations',
                content: {
                    key: 'next',
                    content: this.getRenderRow(this.state.lastMonthEvents, lastMonthEvents),
                },
            }
        ]
        if (this.state.loader) {
            return (
                <div className="loaderContainer">
                    <Loader className="eventsLoader" />
                </div>
            );
        } else {
            return (
                <div>
                    <div>
                        <div>
                            <Button content="Change Message Target" id="changeMessageTargetBtn" onClick={this.onChangeMessageTarget} primary />
                            <Text className="message-target-channel-name">Message target channel:&nbsp;{this.state.messageTargetChannel}</Text>
                        </div>
                        <hr/>
                    </div>
                    <div className="teamEventsContainer">
                        <Accordion defaultActiveIndex={[0, 1]} panels={panels} />
                    </div>
                </div>
            );
        }
    }

    private getEventsFromApi = async () => {
        try {
            const currentMonthResponse = await getTeamEvents(this.state.currentMonthStart, this.state.currentMonthEnd, this.state.teamId);
            const lastMonthResponse = await getTeamEvents(this.state.nextMonthStart, this.state.nextMonthEnd, this.state.teamId);
            const currentMessageTarget = await getMessageTarget(this.state.teamId);

            this.setState({
                currentMonthEvents: currentMonthResponse.data,
                lastMonthEvents: lastMonthResponse.data,
                errorState: false,
                messageTargetChannel: currentMessageTarget.data,
            });
        } catch (error) {
            this.setState({
                errorState: true,
            })
            return error;
        }
    }

    private getTableHeader = () => {
        const header = {
            items: [
                {
                    content: (
                        <div className="avatarCell">
                            <Text>Owner</Text>
                        </div>
                    ),
                    key: 'ownerHeader',
                    className: 'avatar'
                },
                {
                    content: 'Title',
                    key: 'nameHeader',
                },
                {
                    content: 'Date',
                    key: 'dateHeader',
                },
                {
                    content: 'Type',
                    key: 'typeHeader',
                },
                {
                    content: 'Message',
                    key: 'messageHeader',
                }
            ]
        }
        return header;
    }

    private renderRow = (events: any, result: any) => {
        if (events.length > 0) {
            events.forEach((item: any, index: string) => {
                let event = {
                    key: index,
                    items: [
                        {
                            content: (
                                <div className="avatarCell">
                                    <Avatar name={item.owner} />
                                </div>
                            ),
                            key: 'owner',
                            className: 'avatar'
                        },
                        {
                            content: item.title,
                            key: 'name',
                            truncateContent: true,
                        },
                        {
                            content: (new Date(item.date).getMonth() + 1) + '-' + new Date(item.date).getDate(),
                            key: 'date',
                        },
                        {
                            content: item.eventType,
                            key: 'type',
                        },
                        {
                            content: item.message,
                            key: 'message',
                            truncateContent: true,
                        },
                    ],
                    className: 'rowEvent',
                };
                result.push(event);
            });
        }
    }

    private getRenderRow = (events: any, data: any): JSX.Element => {
        if (!events || events.length == 0) {
            return (<Text content="No coming celebrations." weight="bold"></Text>);
        } else {
            return (
                <div className="teamEvents">
                    <Table header={this.getTableHeader()} rows={data} />
                </div>);
        }
    }

    // Handles the click event (coming from the change message target button).
    // The function gets an adaptive card from back-end.
    // The card contains the channel list of the current team.
    // User can choose a channel, and set it as the message target channel.
    // The function calls the web API to save the change to the message target.
    private onChangeMessageTarget = async (): Promise<any> => {
        const teamId = this.state.teamId;

        const messageTargetInfo = await getChangeMessageTargetCard(teamId);
        const card = messageTargetInfo.data;

        const submitHandler = async (err: any, result: any): Promise<void> => {
            if (result && result.ChangeMessageTargetChoiceSetInputId) {
                const saveMessageTargetInfoResult = await saveMessageTargetInfo(teamId, result.ChangeMessageTargetChoiceSetInputId);
                this.setState({
                    messageTargetChannel: saveMessageTargetInfoResult.data
                });
            }
        };

        const taskInfo: TaskInfo = {
            card: card,
            title: "Change Message Target",
            height: 200,
            width: 400
        };

        microsoftTeams.tasks.startTask(taskInfo, submitHandler);
    }
}

export default TeamEvents;