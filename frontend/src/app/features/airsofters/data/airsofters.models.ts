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
