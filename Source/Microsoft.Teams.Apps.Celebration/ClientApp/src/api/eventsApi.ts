import axios from './axiosJWTDecorator';
import { getBaseUrl } from '../configVariables';

let baseAxiosUrl = getBaseUrl() + '/api';

export const getCelebrationsEvents = async (): Promise<any> => {
    let url = baseAxiosUrl + "/events";
    return await axios.get(url);
}

export const getEvent = async (id: string): Promise<any> => {
    let url = baseAxiosUrl + "/events/" + id;
    return await axios.get(url);
}

export const deleteCEvent = async (id: string): Promise<any> => {
    let url = baseAxiosUrl + "/events/" + id;
    return await axios.delete(url);
}

export const addNewCEvent = async (payload: {}): Promise<any> => {
    let url = baseAxiosUrl + "/events";
    return await axios.post(url, payload);
}

export const updateEvent = async (payload: {}): Promise<any> => {
    let url = baseAxiosUrl + "/events";
    return await axios.put(url, payload);
}

export const getTimezones = async (): Promise<any> => {
    let url = baseAxiosUrl + "/events/timezones";
    return await axios.get(url);
}

export const getTeams = async (): Promise<any> => {
    let url = baseAxiosUrl + "/teamdata";
    return await axios.get(url);
}

export const getTeamEvents = async (startTime: string, endTime: string, teamId: string): Promise<any> => {
    let url = baseAxiosUrl + "/events/from/" + startTime + "/to/" + endTime + "/team/" + teamId;
    return await axios.get(url);
}

export const getChangeMessageTargetCard = async (teamId: string): Promise<any> => {
    let url = baseAxiosUrl + "/teamdata/" + teamId + "/message-target-card";
    return await axios.get(url);
}

export const saveMessageTargetInfo = async (teamId: string, targetChannelId: string): Promise<any> => {
    let url = baseAxiosUrl + "/teamdata/" + teamId + "/message-target/" + targetChannelId;
    return await axios.put(url);
}

export const getMessageTarget = async (teamId: string): Promise<any> => {
    let url = baseAxiosUrl + "/teamdata/" + teamId + "/message-target";
    return await axios.get(url);
}

export const getAuthenticationConsentMetadata = async (windowLocationOriginDomain: string, login_hint: string): Promise<any> => {
    let url = `${baseAxiosUrl}/authenticationMetadata/consentUrl?windowLocationOriginDomain=${windowLocationOriginDomain}&loginhint=${login_hint}`;
    return await axios.get(url, undefined, false);
}