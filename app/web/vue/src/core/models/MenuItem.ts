/**
 * Menu item
 */
declare interface MenuItem {
  heading?: string;
  route?: string;
  pages: [] | MenuSubItem[];
  permissions?: string[];
}

declare interface MenuSubItem {
  heading?: string;
  route: string;
  svgIcon: string;
  fontIcon?: string;
  permissions: string[];
  pages?: MenuSubItem[];
  sitePermissions?: string[];
  tenantPermissions?: string[];
}
export default MenuItem;
