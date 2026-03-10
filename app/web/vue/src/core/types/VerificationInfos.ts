export enum VerificationStatusTypes {
  Incomplete = 0,
  AwaitingReview = 1,
  UnderReview = 2,
  AwaitingApprove = 3,
  Approved = 4,
  Rejected = 5,

  AwaitingAddressVerify = 6,
  AwaitingCodeVerify = 7,
  CodeVerified = 8,
}

export enum VerificationTypes {
  Undefined = -1,
  Unknown = 0,
  Verification = 1,
  KycForm = 2,
}

export enum VerificationDocumentTypes {
  IdFront = "id_front",
  IdBack = "id_back",
  Address = "address",
  Other = "other",
}

export enum VerificationStatus {
  incomplete = VerificationStatusTypes.Incomplete,
  awaitingReview = VerificationStatusTypes.AwaitingReview,
  underReview = VerificationStatusTypes.UnderReview,
  awaitingApprove = VerificationStatusTypes.AwaitingApprove,
  approved = VerificationStatusTypes.Approved,
  rejected = VerificationStatusTypes.Rejected,
  awaitingIdVerify = VerificationStatusTypes.AwaitingAddressVerify,
  awaitingCodeVerify = VerificationStatusTypes.AwaitingCodeVerify,
  codeVerified = VerificationStatusTypes.CodeVerified,
}
