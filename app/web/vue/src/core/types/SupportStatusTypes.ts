export enum SupportStatusTypes {
  Pending = 1,
  InProgress = 2,
  Resolved = 3,
}

export enum ClientView {
  CaseList = 0,
  CaseDetail = 1,
  CreateCase = 2,
}

export enum CaseStatusTypes {
  Created = 0,
  Processing = 10,
  Replied = 20,
  Closed = 30,
}

export enum SupportTabStatus {
  Pending = CaseStatusTypes.Created,
  Resolved = CaseStatusTypes.Closed,
  All = 1,
}
