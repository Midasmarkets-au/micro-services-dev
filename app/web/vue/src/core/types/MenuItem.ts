/**
 * Menu item
 */
declare interface MenuItem {
  sectionTitle?: string;
  heading?: string;
  route?: string;
  permissions?: string[];
  stat?: string;
  pages?: [] | MenuSubItem[];
  sub?: MenuSubItem[];
}

declare interface MenuSubItem {
  heading?: string;
  sectionTitle?: string;
  route: string;
  svgIcon: string;
  fontIcon?: string;
  stat?: string;
  permissions: string[];
  sub?: MenuSubItem[];
}
export default MenuItem;
