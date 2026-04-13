import store from "@/store";

// Token is in HttpOnly cookie — browser sends it automatically with withCredentials requests.
// No need to append access_token as a query param.
export const getImageUrlWithToken = (guid: string) => {
  if (guid === "no-image") return "/images/no-image.png";
  if (guid === null || guid === "" || guid === "no-avatar") {
    return "/images/avatar.png";
  }

  const roles = store.state.AuthModule?.user?.roles;

  return `${window["api"]}/api/v1/${
    roles && roles.includes("TenantAdmin") ? "tenant" : "client"
  }/media/${guid}`;
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
