<template>
  <!--begin::sidebar menu-->
  <div class="app-sidebar-menu overflow-hidden flex-column-fluid">
    <!--begin::Menu wrapper-->
    <div
      id="kt_app_sidebar_menu_wrapper"
      class="app-sidebar-wrapper hover-scroll-overlay-y my-5"
      data-kt-scroll="true"
      data-kt-scroll-activate="true"
      data-kt-scroll-height="auto"
      data-kt-scroll-dependencies="#kt_app_sidebar_logo, #kt_app_sidebar_footer"
      data-kt-scroll-wrappers="#kt_app_sidebar_menu"
      data-kt-scroll-offset="5px"
      data-kt-scroll-save-state="true"
    >
      <!--begin::Menu-->
      <div
        id="#kt_app_sidebar_menu"
        class="menu menu-column menu-rounded menu-sub-indention px-3"
        data-kt-menu="true"
      >
        <template v-for="(item, i) in MainMenuConfig" :key="i">
          <!-- 03032025 - compliance to ASIC Demo: hide rebate menu for AU ff-->
          <template
            v-if="
              $cans(item.permissions) &&
              !(item.heading === 'title.rebate' && tenancy === 'au')
            "
          >
            <div v-if="item.heading" class="menu-item pt-1">
              <div class="menu-content">
                <span class="menu-heading fw-bold text-uppercase fs-7">
                  {{ translate(item.heading) }}
                </span>
              </div>
            </div>
            <template v-for="(menuItem, j) in item.pages" :key="j">
              <template
                v-if="
                  menuItem.heading &&
                  $cans(menuItem.permissions) &&
                  !(menuItem.heading === 'title.crypto' && tenancy === 'au')
                "
              >
                <div class="menu-item">
                  <router-link
                    :to="menuItem.route"
                    :key="menuItem.route"
                    class="menu-link"
                    :class="{ active: isActive(menuItem.route) }"
                  >
                    <span
                      v-if="menuItem.svgIcon || menuItem.fontIcon"
                      class="menu-icon"
                      style=""
                    >
                      <i
                        v-if="sidebarMenuIcons === 'font'"
                        :class="menuItem.fontIcon"
                        class="bi fs-3"
                      ></i>
                      <span
                        v-else-if="sidebarMenuIcons === 'svg'"
                        class="svg-icon svg-icon-2"
                      >
                        <inline-svg :src="menuItem.svgIcon" />
                      </span>
                    </span>
                    <span class="menu-title fs-4">
                      {{ translate(menuItem.heading)
                      }}<span
                        class="badge badge-danger badge-circle fw-bolder ms-2"
                        v-if="
                          menuItem.stat != undefined &&
                          menuItem.stat != '' &&
                          getIStat(menuItem.stat) > 0
                        "
                        >{{ getIStat(menuItem.stat) }}</span
                      >
                    </span>
                  </router-link>
                </div>
              </template>
              <div
                v-if="menuItem.sectionTitle"
                :class="{ show: hasActiveChildren(menuItem.route) }"
                class="menu-item menu-accordion"
                data-kt-menu-sub="accordion"
                data-kt-menu-trigger="click"
              >
                <span class="menu-link">
                  <span
                    v-if="menuItem.svgIcon || menuItem.fontIcon"
                    class="menu-icon"
                    style=""
                  >
                    <i
                      v-if="sidebarMenuIcons === 'font'"
                      :class="menuItem.fontIcon"
                      class="bi fs-3"
                    ></i>
                    <span
                      v-else-if="sidebarMenuIcons === 'svg'"
                      class="svg-icon svg-icon-2"
                    >
                      <inline-svg :src="menuItem.svgIcon" />
                    </span>
                  </span>
                  <span class="menu-title fs-4">{{
                    translate(menuItem.sectionTitle)
                  }}</span>
                  <span class="menu-arrow"></span>
                </span>
                <div
                  :class="{ show: hasActiveChildren(menuItem.route) }"
                  class="menu-sub menu-sub-accordion"
                >
                  <template v-for="(item2, k) in menuItem.sub" :key="k">
                    <div
                      v-if="item2.heading && $cans(menuItem.permissions)"
                      class="menu-item"
                    >
                      <router-link
                        class="menu-link"
                        active-class="active"
                        :to="item2.route"
                      >
                        <span class="menu-bullet">
                          <span class="bullet bullet-dot"></span>
                        </span>
                        <span class="menu-title fs-4">
                          {{ translate(item2.heading) }}
                        </span>
                      </router-link>
                    </div>
                    <div
                      v-if="item2.sectionTitle"
                      :class="{ show: hasActiveChildren(item2.route) }"
                      class="menu-item menu-accordion"
                      data-kt-menu-sub="accordion"
                      data-kt-menu-trigger="click"
                    >
                      <span class="menu-link">
                        <span class="menu-bullet">
                          <span class="bullet bullet-dot"></span>
                        </span>
                        <span class="menu-title fs-4"
                          >{{ translate(item2.sectionTitle) }}
                        </span>
                        <span class="menu-arrow"></span>
                      </span>
                      <div
                        :class="{ show: hasActiveChildren(item2.route) }"
                        class="menu-sub menu-sub-accordion"
                      >
                        <template v-for="(item3, k) in item2.sub" :key="k">
                          <div
                            v-if="item3.heading && $cans(menuItem.permissions)"
                            class="menu-item"
                          >
                            <router-link
                              class="menu-link"
                              active-class="active"
                              :to="item3.route"
                            >
                              <span class="menu-bullet">
                                <span class="bullet bullet-dot"></span>
                              </span>
                              <span class="menu-title fs-4">{{
                                translate(item3.heading)
                              }}</span>
                            </router-link>
                          </div>
                        </template>
                      </div>
                    </div>
                  </template>
                </div>
              </div>
            </template>
          </template>
        </template>
      </div>
      <!--end::Menu-->
    </div>
    <!--end::Menu wrapper-->
  </div>
  <!--end::sidebar menu-->
</template>

<script>
import { defineComponent, onMounted, ref, inject, watch } from "vue";
import { useRoute } from "vue-router";
import { sidebarMenuIcons } from "../config/config";
import { useI18n } from "vue-i18n";

import { useStore } from "@/store";

export default defineComponent({
  name: "sidebar-menu",
  components: {},
  setup() {
    const { t, te } = useI18n();
    const route = useRoute();
    const store = useStore();
    const tenancy = store.state.AuthModule.user.tenancy;
    const iStat = ref(store.state.TenantModule.iStat);

    const scrollElRef = (ref < null) | (HTMLElement > null);
    const mainMenu = inject("backendMainMenu");
    const MainMenuConfig = mainMenu.menus;
    onMounted(() => {
      if (scrollElRef.value) {
        scrollElRef.value.scrollTop = 0;
      }
    });

    watch(store.state.TenantModule.iStat, (newVal) => {
      iStat.value = newVal;
    });

    const getIStat = (key) => {
      if (iStat.value[key] != undefined) return iStat.value[key];
      else return 0;
    };

    const translate = (text) => {
      if (te(text)) {
        return t(text);
      } else {
        return text;
      }
    };

    const hasActiveChildren = (match) => {
      return route.path.indexOf(match) !== -1;
    };

    const isActive = (menuItemRoute) => {
      const currentPath = route.path;
      // Handle the special case for root and dashboard
      if (
        menuItemRoute === "/" &&
        (currentPath === "/" || currentPath === "/dashboard")
      ) {
        return true;
      }

      // Standard case
      return currentPath === menuItemRoute;
    };
    return {
      hasActiveChildren,
      MainMenuConfig,
      sidebarMenuIcons,
      translate,
      getIStat,
      isActive,
      tenancy,
    };
  },
});
</script>
