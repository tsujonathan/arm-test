import React from 'react';
import { Menu } from '@fluentui/react';
import { getBaseUrl } from '../../configVariables';
import * as microsoftTeams from "@microsoft/teams-js";
import { } from '../../api/eventsApi';

export interface OverflowProps {
    event?: any;
    styles?: object;
    title?: string;
    refreshCard?: any;
}

export interface OverflowState {
    menuOpen: boolean;
}

export interface ITaskInfo {
    title?: string;
    height?: number;
    width?: number;
    url?: string;
    card?: string;
    fallbackUrl?: string;
    completionBotId?: string;
}

class EventOverflow extends React.Component<OverflowProps, OverflowState> {
    constructor(props: OverflowProps) {
        super(props);
        this.state = {
            menuOpen: false,
        };
    }

    public componentDidMount() {
        microsoftTeams.initialize();
    }

    public render(): JSX.Element {
        const items = [
            {
                key: 'more',
                icon: {
                    name: 'more',
                    outline: true,
                },
                indicator: false,
                menu: {
                    items: [
                        {
                            key: 'edit',
                            content: 'Edit',
                            onClick: (event: any) => {
                                event.preventDefault();
                                event.stopPropagation();
                                this.setState({
                                    menuOpen: false,
                                });
                                let url = getBaseUrl() + "/Home/editEvent/" + this.props.event;
                                this.onOpenTaskModule(null, url, "Edit event", 660);
                            }
                        },
                        {
                            key: 'delete',
                            content: 'Delete',
                            onClick: (event: any) => {
                                event.preventDefault();
                                event.stopPropagation();
                                this.setState({
                                    menuOpen: false,
                                });
                                let url = getBaseUrl() + "/Home/deletEvent/" + this.props.event;
                                this.onOpenTaskModule(null, url, "Delete event", 525);

                            }
                        },
                    ],
                },
            },
        ];

        return <Menu className="event-card-overflow" iconOnly items={items} styles={this.props.styles} title={this.props.title} />;
    }

    private onOpenTaskModule = (event: any, url: string, title: string, height: number) => {
        let taskInfo: ITaskInfo = {
            url: url,
            title: title,
            height: height,
            width: 600,
            fallbackUrl: url,
        };
        let submitHandler = (err: any, result: any) => {
            this.props.refreshCard();
        };
        microsoftTeams.tasks.startTask(taskInfo, submitHandler);
    }
}

export default EventOverflow;
