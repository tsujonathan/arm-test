import * as React from 'react';
import './errorState.scss';
import { Button, Text } from '@fluentui/react';
const ErrorImg = require('../../images/ErrorState.png');

export interface INoEventProps {
    refreshList?: any;
}

class ErrorScreen extends React.Component<INoEventProps, {}> {

    public render(): JSX.Element {
        return (
            <div id="divErrorStateforTab" className="error-state-div">
                <div id="errorEvent">
                    <div>
                        <img src={ErrorImg} alt="errorEvent" className="error-event-image" />
                    </div>
                    <Text className="error-event-header" content="Uh oh ... we're having trouble retrieving your information."></Text>
                    <div className="error-event-regular-text">
                        <Text content="Click Retry to have another go at it"></Text>
                    </div>
                    <div className="retry-button">
                        <Button content="Retry" onClick={() => this.props.refreshList()} primary />
                    </div>
                </div>
            </div>
        );
    }
}

export default ErrorScreen;