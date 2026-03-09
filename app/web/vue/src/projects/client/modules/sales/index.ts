import registerRoute from "./routes";
// import registerMenu from "./menu";
import SalesStoreModule from "./store/SalesModule";

const SalesModule = {
  install: function (app, options) {
    registerRoute(options.router);
    // registerMenu(options.menu);
    options.store.registerModule("SalesModule", SalesStoreModule);
  },
};

export default SalesModule;
