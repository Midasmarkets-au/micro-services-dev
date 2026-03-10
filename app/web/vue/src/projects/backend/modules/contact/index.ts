import registerRoute from "./routes";
import registerMenu from "./menu";

const ContactModule = {
  install: function (app, options) {
    registerRoute(options.router);
    registerMenu(options.menu);
  },
};
export default ContactModule;
