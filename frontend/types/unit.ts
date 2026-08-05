export interface Unit {
  name: string;
  description: string;
  locationName: string;
  locationDn?: string;
  userCount: number;
}

export interface CreateUnitRequest {
  name: string;
  description: string;
  locationName: string;
}

export interface UpdateUnitRequest {
  name: string;
  description: string;
  locationName: string;
}

export interface UnitApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
}