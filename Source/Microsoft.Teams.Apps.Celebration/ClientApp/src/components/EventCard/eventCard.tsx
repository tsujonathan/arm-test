import * as React from 'react';
import './eventCard.scss';
import { Text } from '@fluentui/react';
import EventOverflow from '../EventOverflow/eventOverflow';
const NewEvent = require('../../images/Celebrations-bot-image-new-event.jpg');

export interface IEventCardProps {
    cardType: string;
    newEvent?: any;
    id?: string;
    title?: string;
    date?: string;
    image?: string;
    message?: string;
    refreshList?: any;
}

export interface IEventCardState {
    id: string;
    title: string;
    date: string;
    image: string;
    message: string;
}

enum cardType {
    eventCard = 'eventCard',
}

class EventCard extends React.Component<IEventCardProps, IEventCardState> {

    constructor(props: IEventCardProps) {
        super(props);
        this.state = {
            id: "",
            title: "Event title",
            date: "Date",
            image: NewEvent,
            message: "Say something about your event.",
        }
    }

    public componentDidMount() {
        if (this.props.cardType === cardType.eventCard && this.props.id && this.props.title && this.props.date && this.props.image !== undefined && this.props.message) {
            let resultDate = "";
            if (this.props.date) {
                resultDate = (new Date(this.props.date)).toLocaleString(navigator.language, { month: 'long', day: 'numeric' });
            }
            this.setState({
                id: this.props.id,
                title: this.props.title,
                date: resultDate,
                image: require(`../../images/Carousel/Celebrations-bot-image-${this.props.image}-.png`),
                message: this.props.message,
            });
        }
    }

    public componentWillReceiveProps(nextProps: any) {
        if (nextProps.cardType === "eventCard") {
            let resultDate = "";
            if (nextProps.date) {
                resultDate = (new Date(nextProps.date)).toLocaleString(navigator.language, { month: 'long', day: 'numeric' });
            }
            this.setState({
                id: nextProps.id,
                title: nextProps.title,
                date: resultDate,
                image: require(`../../images/Carousel/Celebrations-bot-image-${nextProps.image}-.png`),
                message: nextProps.message,
            });
        }
    }

    public render(): JSX.Element {
        const hide: any = {
            visibility: "hidden",
        }
        const show: any = {
            visibility: "visible",
        }
        return (
            <div tabIndex={0} className="event-card" onKeyPress={this.keyPressded} onClick={() => this.props.newEvent(this.state.id)}>
                <Text className="event-card-date ghost-tile-text" content={this.state.date}></Text>
                <EventOverflow event={this.state.id} refreshCard={this.refresh} styles={this.props.cardType === "newCard" ? hide : show}></EventOverflow>
                <Text className="event-card-title ghost-tile-text" content={this.state.title} truncated weight="bold" ></Text>
                <div className="event-card-celebration-image-div">
                    <img src={this.state.image} alt="" />
                </div>
                <Text className="event-card-message-header ghost-tile-text" content="Message"></Text>
                <Text className="event-card-message ghost-tile-text" content={this.state.message} truncated></Text>
            </div>
        );
    }

    public keyPressded = (event: React.KeyboardEvent<HTMLDivElement>) => {
        if (event.key === "Enter") {
            this.props.newEvent(this.state.id);
        }
    }

    public refresh = () => {
        this.props.refreshList();
    }
}

export default EventCard;