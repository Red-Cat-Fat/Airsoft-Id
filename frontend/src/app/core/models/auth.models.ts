export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: Date;
  user: UserInfo;
}

export interface UserInfo {
  id: string;
  email: string;
  callsign: string;
  roles: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  middleName?: string;
  callsign: string;
}
