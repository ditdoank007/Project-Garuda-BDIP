export interface SynologyImportPreviewUser {
  rowNumber: number;
  sourceUsername: string;
  username: string;
  fullName: string;
  email: string;
  status: string;
  enabled: boolean;
  groups: string[];
  isDuplicateInCsv: boolean;
  existsInLdap: boolean;
  action: string;
  note: string;
}

export interface SynologyImportPreview {
  totalRows: number;
  validRows: number;
  newUsers: number;
  existingUsers: number;
  duplicateUsernames: number;
  usersWithoutEmail: number;
  disabledUsers: number;
  groupsFound: string[];
  users: SynologyImportPreviewUser[];
}

export interface SynologyImportResult {
  totalRows: number;
  createdUsers: number;
  skippedExistingUsers: number;
  disabledUsers: number;
  groupMembershipsAdded: number;
  errors: string[];
}

export interface SynologyUploadResponse {
  uploadId: string;
  fileName: string;
  fileSize: number;
}
