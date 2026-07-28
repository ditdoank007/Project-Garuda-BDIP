export interface Unit {
  name: string;
  description: string;
  userCount: number;
}

export interface CreateUnitRequest {
  name: string;
  description: string;
}

export interface UpdateUnitRequest {
  name: string;
  description: string;
}

export interface UnitApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
}
