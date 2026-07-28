export interface Location {
  name: string;
  description: string;
  type: string;
  unitCount: number;
}

export interface LocationFormData {
  name: string;
  description: string;
  type: string;
}

export interface LocationListResponse {
  success: boolean;
  data: Location[];
}