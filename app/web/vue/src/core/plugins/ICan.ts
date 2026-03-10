import { App } from "vue";

// import { useStore } from "@/store";
import store from "@/store";
import { RoleTypes } from "@/core/types/RoleTypes";

interface ICan {
  install: (app: App) => void;
  cans: (value: string[]) => boolean;
  can: (value: string) => boolean;
}

// function can(act: string): boolean {
//   console.log("can test", act);
//   return false;
// }
const Can: ICan = {
  install: function (app: App) {
    app.config.globalProperties.$can = Can.can;
    app.config.globalProperties.$cans = Can.cans;
  },
  can: function (value: string): boolean {
    return Can.cans([value]);
  },
  cans: function (value: string[]): boolean {
    // const store = useStore();
    if (value == undefined) return false;
    const user = store.state.AuthModule.user;
    if (user && user.uid > 0) {
      if (user.roles.includes(RoleTypes.SuperAdmin)) {
        return true;
      }
      const roles = user.roles;
      const permissions = user.permissions;
      if (permissions !== undefined) {
        for (let i = 0; i < value.length; i++) {
          const element = value[i];
          if (element === "any") {
            return true;
          }
          if (roles.includes(element)) {
            return true;
          }
          if (permissions.includes(element)) {
            return true;
          }
        }
      } else {
        return false;
      }
    }
    return false;
  },
};

declare module "@vue/runtime-core" {
  interface ComponentCustomProperties {
    $can: ICan["can"];
    $cans: ICan["cans"];
  }
}
export default Can;
