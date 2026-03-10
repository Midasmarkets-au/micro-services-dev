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
};

export default apis;
