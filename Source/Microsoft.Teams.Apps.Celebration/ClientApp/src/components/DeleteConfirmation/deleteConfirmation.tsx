import * as React from 'react';
import './deleteConfirmation.scss';
import { RouteComponentProps } from 'react-router-dom';
import { Loader, Button, Text } from '@fluentui/react';
import * as microsoftTeams from "@microsoft/teams-js";
import { getEvent, deleteCEvent } from '../../api/eventsApi';

interface IDeleteEventState {
    event: any;
    loader: boolean;
}

class DeleteConfirmation extends React.Component<RouteComponentProps, IDeleteEventState> {
    private initEvent = {
        id: "",
        title: "",
        image: 0,
    };

    constructor(props: RouteComponentProps) {
        super(props);

        this.state = {
            event: this.initEvent,
            loader: true,
        };
    }

    public componentDidMount() {
        microsoftTeams.initialize();

        let params = this.props.match.params;
        if ('id' in params) {
            let id = params['id'];
            this.getItem(id).then(() => {
                this.setState({
                    loader: false,
                })
            });
        }
    }

    private getItem = async (id: string) => {
        try {
            const response = await getEvent(id);
            this.setState({
                event: response.data,
            });
        } catch (error) {
            return error;
        }
    }

    public render(): JSX.Element {
        if (this.state.loader) {
            return (
                <div className="taskmoduleLoader">
                    <Loader />
                </div>
            );
        } else {
            return (
                <div className="deleteTaskModule">
                    <div className="formContainer">
                        <div className="formContentContainer" >
                            <div className="contentField">
                                <h3>Want to remove this event?</h3>
                                <span>The {this.state.event.title} event will be permanently deleted.</span>
                                <div tabIndex={0} className="event-card" >
                                    <Text className="event-card-date ghost-tile-text" content={(new Date(this.state.event.date)).toLocaleString(navigator.language, { month: 'long', day: 'numeric' })}></Text>
                                    <Text className="event-card-title ghost-tile-text" content={this.state.event.title} truncated weight="bold"></Text>
                                    <div className="event-card-celebration-image-div">
                                        <img src={require(`../../images/Carousel/Celebrations-bot-image-${this.state.event.image}-.png`)} alt="" />
                                    </div>
                                    <Text className="event-card-message-header ghost-tile-text" content="Message"></Text>
                                    <Text className="event-card-message ghost-tile-text" content={this.state.event.message} truncated></Text>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div className="footerContainer">
                        <div className="buttonContainer">
                            <Loader id="sendingLoader" className="hiddenLoader sendingLoader" size="smallest" label="Deleting event" labelPosition="end" />
                            <Button content="Delete" id="sendBtn" onClick={this.onDelete} primary />
                        </div>
                    </div>
                </div>
            );
        }
    }

    private onDelete = () => {
        let spanner = document.getElementsByClassName("sendingLoader");
        spanner[0].classList.remove("hiddenLoader");
        deleteCEvent(this.state.event.id).then(() => {
            microsoftTeams.tasks.submitTask();
        });

    }
}

export default DeleteConfirmation;