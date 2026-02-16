export interface Airsofter {
  id: string;
  firstName: string;
  lastName: string;
  middleName?: string | null;
  callsign: string;
  kitDescription?: string | null;
  airsoftGunsList?: string | null;
  teamId?: string | null;
  teamName?: string | null;
  gamesPlayed: number;
  violationsCount: number;
}

export interface AirsofterGameHistoryItem {
  id: string;
  date: string;
  name: string;
  status: string;
  registeredAt: string;
}

export interface AirsofterViolation {
  id: string;
  description: string;
  date: string;
  severity: string;
  fieldName: string;
}
