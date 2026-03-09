import { App } from "vue";
import MenuItem from "@/core/models/MenuItem";
import { DocMenu, SystemMenu } from "@/core/config/MainMenuConfig";

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
      app.provide("mainMenu", menu);
      menu.loadMenu();
    },
  };
  return menu;
}

export { createMenu, Menu };
