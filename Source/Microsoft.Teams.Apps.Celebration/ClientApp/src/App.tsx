import React from 'react';
import { BrowserRouter, Route, Switch } from 'react-router-dom';
import { Provider, themes } from '@fluentui/react';
import { TeamsThemeContext, getContext, ThemeStyle } from 'msteams-ui-components-react';
import * as microsoftTeams from "@microsoft/teams-js";
import Events from './components/Events/events';
import editEvent from './components/EditEventTaskModule/editEvent';
import DeleteConfirmation from './components/DeleteConfirmation/deleteConfirmation';
import './App.scss';
import SignInPage from "./components/SignInPage/signInPage";
import SignInSimpleStart from "./components/SignInPage/signInSimpleStart";
import SignInSimpleEnd from "./components/SignInPage/signInSimpleEnd";
import AuthErrorPage from "./components/AuthErrorPage/autherrorPage";
import Configuration from './components/config';
import TeamEvents from './components/TeamEvents/teamEvents';

export interface IAppState {
  theme: string;
  themeStyle: number;
}

class App extends React.Component<{}, IAppState>{
  constructor(props: {}) {
    super(props);
    this.state = {
      theme: "",
      themeStyle: ThemeStyle.Light,
    }
  }

  public componentDidMount() {
    microsoftTeams.initialize();
    microsoftTeams.getContext((context) => {
      let theme = context.theme || "";
      this.updateTheme(theme);
      this.setState({
        theme: theme
      });
    });

    microsoftTeams.registerOnThemeChangeHandler((theme) => {
      this.updateTheme(theme);
      this.setState({
        theme: theme,
      }, () => {
        this.forceUpdate();
      });
    });
  }

  public setThemeComponent = () => {
    if (this.state.theme === "dark") {
      return (
        <Provider theme={themes.teamsDark}>
          <div className="darkContainer">
            {this.getAppDom()}
          </div>
        </Provider>
      );
    }
    else if (this.state.theme === "contrast") {
      return (
        <Provider theme={themes.teamsHighContrast}>
          <div className="highContrastContainer">
            {this.getAppDom()}
          </div>
        </Provider>
      );
    } else {
      return (
        <Provider theme={themes.teams}>
          <div className="defaultContainer">
            {this.getAppDom()}
          </div>
        </Provider>
      );
    }
  }

  private updateTheme = (theme: string) => {
    if (theme === "dark") {
      this.setState({
        themeStyle: ThemeStyle.Dark
      });
    } else if (theme === "contrast") {
      this.setState({
        themeStyle: ThemeStyle.HighContrast
      });
    } else {
      this.setState({
        themeStyle: ThemeStyle.Light
      });
    }
  }

  public getAppDom = () => {
    const context = getContext({
      baseFontSize: 10,
      style: this.state.themeStyle
    });
    return (
      <TeamsThemeContext.Provider value={context}>
        <div className="appContainer">
          <BrowserRouter>
            <Switch>
              <Route exact path="/configtab" component={Configuration} />
              <Route exact path="/Home/Events" component={Events} />
              <Route exact path="/Home/teamEvents" component={TeamEvents} />
              <Route exact path="/Home/editEvent" component={editEvent} />
              <Route exact path="/Home/editEvent/:id" component={editEvent} />
              <Route exact path="/Home/deletEvent/:id" component={DeleteConfirmation} />
              <Route exact path="/signin" component={SignInPage} />
              <Route exact path="/signin-simple-start" component={SignInSimpleStart} />
              <Route exact path="/signin-simple-end" component={SignInSimpleEnd} />
              <Route exact path="/errorpage" component={AuthErrorPage} />
              <Route exact path="/errorpage/:code" component={AuthErrorPage} />
            </Switch>
          </BrowserRouter>
        </div>
      </TeamsThemeContext.Provider>
    );
  }

  public render(): JSX.Element {
    return (
      <div>
        {this.setThemeComponent()}
      </div>
    );
  }
}

export default App;
