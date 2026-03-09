const prefixUrl = "/tenant";
const apis = {
  role: [
    {
      name: "list",
      method: "get",
      imageGuid: "/roles",
      params: {},
    },
    {
      name: "create",
      method: "post",
      imageGuid: "/roles",
      params: {
        name: "",
        permissions: [],
      },
    },
    {
      name: "update",
      method: "put",
      imageGuid: "/roles/{id}",
      params: {
        name: "",
        permissions: [],
      },
    },
  ],
  user: [
    {
      name: "list",
      method: "get",
      imageGuid: "/users",
      params: {},
    },
    {
      name: "create",
      method: "post",
      imageGuid: "/users",
      params: {
        name: "",
        email: "",
        timezone: "",
        lang: "",
      },
    },
    {
      name: "update",
      method: "put",
      imageGuid: "/users/{id}",
      params: {
        name: "",
        email: "",
        timezone: "",
        lang: "",
      },
    },
    {
      name: "getAllUsers",
      method: "get",
      imageGuid: prefixUrl + "/user?Page={page}&Size={pageSize}",
    },
  ],
  permission: [
    {
      name: "list",
      method: "get",
      imageGuid: "/permissions",
      params: {},
    },
  ],

  profile: [
    {
      name: "avatar",
      method: "post",
      imageGuid: "/user/profile/avatar",
      params: {
        avatar: "",
      },
    },
  ],

  files: [
    {
      name: "upload",
      method: "post",
      imageGuid: "/tenant/upload/?type={type}",
      params: {
        file: "",
      },
    },
    {
      name: "deleteFileByUid",
      method: "delete",
      imageGuid: "/tenant/media/{uid}",
      params: {},
    },
  ],
};

export default (api) => {
  api.addApis(apis);
};

export enum CoreApi {
  getUserById = "user.getUserById",
  getAllUsers = "user.getAllUsers",
  uploadDocuments = "files.upload",
  getFileByUid = "files.getFileByUid",
  deleteFileByUid = "files.deleteFileByUid",
}
