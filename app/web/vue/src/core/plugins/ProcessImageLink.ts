import JwtService from "@/core/services/JwtService";

import store from "@/store";

export const getImageUrlWithToken = (guid: string) => {
  if (guid === "no-image") return "/images/no-image.png";
  if (guid === null || guid === "" || guid === "no-avatar") {
    return "/images/avatar.png";
  }

  const roles = store.state.AuthModule?.user?.roles;

  return `${window["api"]}/api/v1/${
    roles && roles.includes("TenantAdmin") ? "tenant" : "client"
  }/media/${guid}?access_token=${JwtService.getToken()}`;
};

export const getImageUrl = (guid: string) => {
  if (guid === "no-image") return "/images/no-image.png";
  if (guid === null || guid === "" || guid === "no-avatar") {
    return "/images/avatar.png";
  }
  const roles = store.state.AuthModule?.user?.roles;

  return `${window["api"]}/api/v1/${
    roles && (roles.includes("TenantAdmin") || roles.includes("AccountAdmin"))
      ? "tenant"
      : "client"
  }/media/${guid}`;
};
