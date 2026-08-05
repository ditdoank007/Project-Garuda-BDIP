export interface User {
  uid: string;
  username: string;
  fullName: string;
  email: string;
  unit: string;
  enabled: boolean;

  policyId?: string;
  policyCode?: string;
}

export interface UserFormData {
  username: string;
  fullName: string;
  email: string;
  unit: string;

  password: string;

  confirmPassword: string;

  enabled: boolean;
}

export interface UserListResponse {
  success: boolean;
  message: string;
  data: {
    users: User[];
    total: number;
  };
}