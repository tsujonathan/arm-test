import * as React from 'react';
import './noEvent.scss';
import { Button, Text } from '@fluentui/react';
const EmptyEvent = require('../../images/EmptyEvent.png');

export interface INoEventProps {
    newEvent?: any;
}

class NoEvent extends React.Component<INoEventProps, {}> {

    public render(): JSX.Element {
        return (
            <div id="divEmptyStateforTab" className="empty-state-div">
                <div id="emptyEvent">
                    <div>
                        <img src={EmptyEvent} alt="EmptyEvent" className="empty-event-image" />
                    </div>
                    <Text className="empty-event-header" content="Start Celebrating with your team today."></Text>
                    <div className="empty-event-regular-text">
                        <Text content="Create share special events"></Text>
                    </div>
                    <div className="new-event-button">
                        <Button content="New event" onClick={() => this.props.newEvent("")} primary />
                    </div>
                </div>
            </div>
        );
    }
}

export default NoEvent;