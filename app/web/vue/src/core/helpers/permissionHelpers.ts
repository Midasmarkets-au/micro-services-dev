export enum permissionIndex {
  transfer = 0,
}

export const processPermission = (permissions: string, index: number) => {
  if (!permissions) {
    return true;
  }
  const character = permissions.charAt(index);
  return character == "1" ? true : false;
};

export const enableTransfer = (permissions: string) => {
  return processPermission(permissions, permissionIndex.transfer);
};
