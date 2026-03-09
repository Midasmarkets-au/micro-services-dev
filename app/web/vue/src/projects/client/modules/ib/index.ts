import registerRoute from "./routes";
import IBModule from "@/projects/client/modules/ib/store/IbModule";
import { State } from "@/store";

const IbModule = {
  install: function (app, options) {
    registerRoute(options.router);
    options.store.registerModule("AgentModule", IBModule);
  },
};
export default IbModule;
