import { App } from "vue";
import MenuItem from "@/core/models/MenuItem";
import { DocMenu, SystemMenu } from "@/core/config/BackendMainMenuConfig";
// class Menu {
//   menus: Array<MenuItem> = [];
//   loadHeaderMenu() {
//     this.menus = DocMenu.concat(SystemMenu);
//   }
// }

interface Menu {
  menus: Array<MenuItem>;
  headMenu: Array<MenuItem>;
  systemMenu: Array<MenuItem>;
  moduleMenu: Array<MenuItem>;
  loadMenu(): void;
  addMenu(_menus: Array<MenuItem>): void;
  install(app: App): void;
}

function createMenu(): Menu {
  const menu: Menu = {
    menus: Array<MenuItem>(),
    headMenu: DocMenu,
    systemMenu: SystemMenu,
    moduleMenu: Array<MenuItem>(),
    loadMenu() {
      this.menus = DocMenu.concat(this.systemMenu);
    },
    addMenu(_menus: Array<MenuItem>) {
      this.moduleMenu = this.moduleMenu.concat(_menus);
      this.menus = DocMenu.concat(this.moduleMenu, this.systemMenu);
    },
    install: (app) => {
      app.config.globalProperties.$mainMenu = menu;
      app.provide("backendMainMenu", menu);
      menu.loadMenu();
    },
  };
  return menu;
}

// const MainMenu = {
//   install: (app: App) => {
//     const mainMenu = new Menu();
//     mainMenu.loadHeaderMenu();
//     app.config.globalProperties.$mainMenu = mainMenu;
//     app.provide("mainMenu", mainMenu);
//   },
// };

export { createMenu, Menu };
