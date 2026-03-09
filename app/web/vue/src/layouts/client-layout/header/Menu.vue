<template>
  <!--begin::Menu wrapper-->
  <div
    class="header-menu flex-grow-1"
    id="kt_header_menu"
    data-kt-drawer="true"
    data-kt-drawer-name="header-menu"
    data-kt-drawer-activate="{default: true, lg: false}"
    data-kt-drawer-overlay="true"
    data-kt-drawer-width="{default:'200px', '300px': '250px'}"
    data-kt-drawer-direction="start"
    data-kt-drawer-toggle="#kt_header_menu_mobile_toggle"
    data-kt-swapper="true"
    data-kt-swapper-mode="prepend"
    data-kt-swapper-parent="{default: '#kt_body', lg: '#kt_header_nav'}"
  >
    <!--begin::Menu-->
    <div
      class="menu flex-grow-1 justify-around menu-rounded menu-column menu-lg-row menu-state-primary menu-arrow-gray-400 fw-semibold my-5 my-lg-0 align-items-stretch px-2 px-lg-0 mh-100vh"
      :class="{}"
      id="#kt_header_menu"
      data-kt-menu="true"
    >
      <!--begin::Mobile Menu item-->
      <template class="d-block d-lg-none">
        <div class="menu-item">
          <div class="menu-content d-flex align-items-center flex-wrap">
            <UserAvatar
              :avatar="user.avatar"
              :name="user.name"
              side="client"
              size="50px"
              rounded
            />
            <div class="d-flex flex-column">
              <div class="fw-bold d-flex align-items-center">
                {{ user.name }}
              </div>
              <a href="#" class="fw-semobold text-muted text-hover-primary">
                {{ user.email }}
              </a>
            </div>
          </div>
        </div>
      </template>
      <template v-for="(item, i) in MainMenuConfig" :key="i">
        <template v-if="!item.heading">
          <template v-for="(menuItem, j) in item.pages" :key="j">
            <div
              v-if="
                menuItem.heading &&
                $cans(menuItem.permissions) &&
                checkTenantPermission(menuItem)
              "
              class="menu-item me-lg-1"
            >
              <router-link
                class="menu-link"
                :to="menuItem.route"
                :active-class="
                  hasActiveChildren(menuItem.route) ? 'active' : ''
                "
              >
                <span class="menu-title"
                  >{{ translate(menuItem.heading) }}
                </span>
              </router-link>
            </div>
          </template>
        </template>

        <div
          v-if="item.heading && $cans(item.permissions)"
          data-kt-menu-trigger="click"
          data-kt-menu-placement="bottom-start"
          class="menu-item menu-lg-down-accordion me-lg-1"
        >
          <span
            class="menu-link py-3"
            :class="{ active: hasActiveChildren(item.route) }"
          >
            <span class="menu-title">{{ translate(item.heading) }}</span>
            <span class="menu-arrow d-lg-none"></span>
          </span>
          <div
            class="menu-sub menu-sub-lg-down-accordion menu-sub-lg-dropdown menu-rounded-0 py-lg-4 w-lg-225px"
          >
            <template v-for="(menuItem, j) in item.pages" :key="j">
              <div
                v-if="menuItem.sectionTitle"
                data-kt-menu-trigger="{default:'click', lg: 'hover'}"
                data-kt-menu-placement="right-start"
                class="menu-item menu-lg-down-accordion"
              >
                <span
                  class="menu-link py-3"
                  :class="{ active: hasActiveChildren(menuItem.route) }"
                >
                  <span class="menu-icon">
                    <i
                      v-if="headerMenuIcons === 'font'"
                      :class="menuItem.fontIcon"
                      class="bi fs-3"
                    ></i>
                    <span
                      v-if="headerMenuIcons === 'svg'"
                      class="svg-icon svg-icon-2"
                    >
                      <inline-svg :src="menuItem.svgIcon" />
                    </span>
                  </span>
                  <span class="menu-title">{{
                    translate(menuItem.sectionTitle)
                  }}</span>
                  <span class="menu-arrow"></span>
                </span>
                <div
                  class="menu-sub menu-sub-lg-down-accordion menu-sub-lg-dropdown menu-active-bg py-lg-4 w-lg-225px"
                >
                  <template v-for="(menuItem1, k) in menuItem.sub" :key="k">
                    <div
                      v-if="menuItem1.sectionTitle"
                      data-kt-menu-trigger="{default:'click', lg: 'hover'}"
                      data-kt-menu-placement="right-start"
                      class="menu-item menu-lg-down-accordion"
                    >
                      <span
                        class="menu-link py-3"
                        :class="{ active: hasActiveChildren(menuItem1.route) }"
                      >
                        <span class="menu-bullet">
                          <span class="bullet bullet-dot"></span>
                        </span>
                        <span class="menu-title">{{
                          translate(menuItem1.sectionTitle)
                        }}</span>
                        <span class="menu-arrow"></span>
                      </span>
                      <div
                        class="menu-sub menu-sub-lg-down-accordion menu-sub-lg-dropdown menu-active-bg py-lg-4 w-lg-225px"
                      >
                        <template
                          v-for="(menuItem2, l) in menuItem1.sub"
                          :key="l"
                        >
                          <div class="menu-item">
                            <router-link
                              class="menu-link py-3"
                              active-class="active"
                              :to="menuItem2.route"
                            >
                              <span class="menu-bullet">
                                <span class="bullet bullet-dot"></span>
                              </span>
                              <span class="menu-title">{{
                                translate(menuItem2.heading)
                              }}</span>
                            </router-link>
                          </div>
                        </template>
                      </div>
                    </div>
                    <div v-if="menuItem1.heading" class="menu-item">
                      <router-link
                        class="menu-link"
                        active-class="active"
                        :to="menuItem1.route"
                      >
                        <span class="menu-bullet">
                          <span class="bullet bullet-dot"></span>
                        </span>
                        <span class="menu-title">{{
                          translate(menuItem1.heading)
                        }}</span>
                      </router-link>
                    </div>
                  </template>
                </div>
              </div>
              <div
                v-if="menuItem.heading && $cans(menuItem.permissions)"
                class="menu-item"
              >
                <router-link
                  class="menu-link"
                  active-class="active"
                  :to="menuItem.route"
                >
                  <span class="menu-icon">
                    <i
                      v-if="headerMenuIcons === 'font'"
                      :class="menuItem.fontIcon"
                      class="bi fs-3"
                    ></i>
                    <span
                      v-if="headerMenuIcons === 'svg'"
                      class="svg-icon svg-icon-2"
                    >
                      <inline-svg :src="menuItem.svgIcon" />
                    </span>
                  </span>
                  <span class="menu-title">{{
                    translate(menuItem.heading)
                  }}</span>
                </router-link>
              </div>
            </template>
          </div>
        </div>
      </template>

      <div v-if="isMobile && $cans(['Sales'])" class="menu-item me-lg-1">
        <router-link class="menu-link" to="/sales" active-class="active">{{
          $t("title.salesCenter")
        }}</router-link>
      </div>

      <div v-if="isMobile && $cans(['Rep'])" class="menu-item me-lg-1">
        <router-link class="menu-link" to="/rep" active-class="active">{{
          $t("title.repCenter")
        }}</router-link>
      </div>

      <div
        v-if="isMobile && $cans(['IB']) && ibEnabled"
        class="menu-item me-lg-1"
      >
        <router-link class="menu-link" to="/ib" active-class="active">{{
          $t("title.ibCenter")
        }}</router-link>
      </div>

      <div
        class="separator my-5"
        style="border-bottom: 1px solid #e4e6ef !important"
      ></div>
      <!--end::Menu-->
      <template class="d-block d-lg-none">
        <!--begin::Menu item-->

        <!--end::Menu item-->

        <div
          class="menu-item"
          data-kt-menu-trigger="hover"
          data-kt-menu-placement="right-start"
          data-kt-menu-flip="center, top"
        >
          <div class="menu-link">
            <span class="menu-title">
              <span class="rounded d-flex align-items-center">
                <img
                  class="w-15px h-15px rounded-1 me-2"
                  :src="currentLanguageLocale.flag"
                  alt="MM Pro"
                />

                <span> {{ currentLanguageLocale.name }}</span>
              </span>
            </span>
          </div>

          <!--begin::Menu sub-->
          <div class="menu-sub menu-sub-dropdown w-225px py-4">
            <!--begin::Menu item-->
            <div
              class="menu-item px-3"
              v-for="(lang, index) in countries"
              :key="index"
            >
              <a
                @click="setLang(lang.code)"
                href="#"
                class="menu-link d-flex px-5"
                :class="{ active: currentLanguage(lang.code) }"
              >
                <span class="symbol symbol-20px me-4">
                  <img class="rounded-1" :src="lang.flag" alt="MM Pro" />
                </span>
                {{ lang.name ?? "Name" }}
              </a>
            </div>
            <!--end::Menu item-->
          </div>
          <!--end::Menu sub-->
        </div>

        <div class="menu-item my-1">
          <router-link to="/profile" class="menu-link" active-class="active">
            <span class="svg-icon svg-icon-4 me-1" :class="{}">
              <inline-svg src="/images/icons/general/setting.svg" />
            </span>
            <span>{{ $t("title.accountSettings") }} </span>
          </router-link>
        </div>
        <!--end::Menu item-->
        <!--begin::Menu item-->
        <div class="menu-item">
          <a @click="signOut()" class="menu-link">
            <span class="svg-icon svg-icon-4 me-1" :class="{}">
              <inline-svg src="/images/icons/general/exit.svg" /> </span
            ><span>{{ $t("action.signOut") }}</span></a
          >
        </div>
        <!--end::Menu item-->
      </template>
    </div>
  </div>
  <!--end::Menu wrapper-->
</template>

<script lang="ts">
import { ref, defineComponent, inject, computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import {} from "vue-router";
import { useStore } from "@/store";
import { useI18n } from "vue-i18n";
import { headerMenuIcons } from "@/core/helpers/config";
import { version } from "@/core/helpers/documentation";
import { Actions } from "@/store/enums/StoreEnums";
import MenuItem from "@/core/models/MenuItem";
import { Menu } from "@/core/plugins/menu";
import {
  ILanguage,
  // LanguageCodes,
  LanguageTypes,
} from "@/core/types/LanguageTypes";
import { isMobile } from "@/core/config/WindowConfig";
import { getTenantLanguagesOptions, jpSites } from "@/core/types/TenantTypes";
export default defineComponent({
  name: "KTMenu",

  components: {},
  setup() {
    const { t, te } = useI18n();
    const route = useRoute();
    const router = useRouter();
    const i18n = useI18n();

    const store = useStore();
    const user = store.state.AuthModule.user;
    const ibEnabled = computed(() => store.state.AuthModule.config?.ibEnabled);

    const mainMenu = inject("mainMenu") as Menu;
    const MainMenuConfig = mainMenu.menus as Array<MenuItem>;
    const tenancy = store.state.AuthModule.user.tenancy;

    const checkJpPermission = () => {
      const hostname = window.location.hostname;
      const env = process.env.VUE_APP_ENV;
      if (jpSites.includes(hostname) || env == "Development") {
        return true;
      }
      return false;
    };
    // check Tenant Permissions
    const checkTenantPermission = (item: any) => {
      if (item.tenantPermissions) {
        let hasPermission = ref(false);

        switch (tenancy) {
          case "jp":
            hasPermission.value = checkJpPermission();
            break;
        }

        if (item.tenantPermissions.includes(tenancy)) {
          hasPermission.value = true;
        } else {
          hasPermission.value = false;
        }

        return hasPermission.value;
      }
      return true;
    };

    const hasActiveChildren = (match) => {
      if (match === "/") {
        return route.path === match;
      }
      return route.path.indexOf(match) !== -1;
    };

    const translate = (text) => {
      if (te(text)) {
        return t(text);
      } else {
        return text;
      }
    };

    const signOut = () => {
      store.dispatch(Actions.LOGOUT).then(() => {
        router.push({ name: "sign-in" });
        // location.reload();
        //inorder to reset css for line 9 "data-kt-drawer-overlay="true""
        // window.location.href = "sign-in";
      });
    };
    let countries: ILanguage[];
    countries = getTenantLanguagesOptions.value;
    const setLang = async (language: string) => {
      await store.dispatch(Actions.SET_LANG, { language: language });
      i18n.locale.value = language;
    };

    const currentLanguage = (language: string) => {
      return i18n.locale.value === language;
    };

    const currentLanguageLocale = computed(() => {
      return (
        countries.find((x) => x.code === i18n.locale.value) ??
        LanguageTypes.enUS
      );
    });

    return {
      signOut,
      setLang,
      isMobile,
      currentLanguage,
      currentLanguageLocale: currentLanguageLocale,
      countries,
      hasActiveChildren,
      headerMenuIcons,
      MainMenuConfig,
      translate,
      version,
      user,
      ibEnabled,
      checkTenantPermission,
    };
  },
});
</script>

<style lang="scss" scoped>
.header-menu {
  padding: 30px 15px;
  width: 100%;
  .menu {
    position: relative;
  }

  .menu-link {
    @media (max-width: 767px) {
      //font-size: 16px;
    }
  }
}

.menu-state-primary .menu-item .menu-link.active .menu-title {
  color: #000f32;
}

.menu-state-primary .menu-item .menu-link.active {
  color: #000f32;
}
</style>
