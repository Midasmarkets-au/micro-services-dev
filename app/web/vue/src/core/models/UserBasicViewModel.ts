export interface HasNativeName {
  nativeName: string;
}

export interface HasLastName {
  lastName: string;
}

export interface HasFirstName {
  firstName: string;
}

export interface HasDisplayName
  extends HasFirstName,
    HasLastName,
    HasNativeName {
  displayName: string;
}

export interface UserBasicViewModel extends HasDisplayName {
  id: number;
  uid: number;
  partyId: number;
  email: string;
  avatar: string;
  hasComment: boolean | null;
}
