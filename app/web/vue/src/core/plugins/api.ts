import { App } from "vue";
import ApiService from "../services/ApiService";
import { apiProviderKey } from "@/core/plugins/providerKeys";

export interface ApiFuncType {
  (options: object | null): Promise<any>;
}

interface Api {
  apis: Array<ApiFuncType>;
  install(app: App): void;
  addApis(_apis: object): void;
}

function createApi(): Api {
  const api: Api = {
    apis: [],
    install(app: App) {
      app.config.globalProperties.$api = api.apis;
      app.provide("api", api.apis);
      app.provide(apiProviderKey, api.apis);
    },
    addApis(_apis: object) {
      Object.keys(_apis).forEach((namespace) => {
        _apis[namespace].forEach((item) => {
          this.apis[namespace + "." + item.name] = (options = {}) => {
            let url = "api/v1" + item.imageGuid;
            let data = "";
            Object.keys(options).forEach((key) => {
              if (key === "data") {
                data = options["data"];
              } else {
                url = url.replace("{" + key + "}", options[key]);
              }
            });
            return ApiService[item.method.toLowerCase()](url, data);
          };
        });
      });
    },
  };
  return api;
}

export default createApi;
