import registerRoute from "./routes";
import registerMenu from "./menu";
const ReportModule = {
  install: function (app, options) {
    registerRoute(options.router);
    registerMenu(options.menu);
  },
};
export default ReportModule;
