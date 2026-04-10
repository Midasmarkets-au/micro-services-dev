<template>
  <div
    class="menu menu-sub menu-sub-dropdown menu-column menu-rounded menu-gray-600 menu-state-bg-light-primary menu-hover-bg fw-semobold py-4 fs-6"
    data-kt-menu="true"
  >
    <div class="menu-item px-3">
      <div class="menu-content d-flex align-items-center px-3">
        <UserAvatar
          :avatar="user?.avatar"
          :name="user?.name"
          size="50px"
          class="me-3"
          side="client"
          v-if="user"
        />
        <div class="d-flex flex-column">
          <div class="fw-bold d-flex align-items-center fs-5">
            {{ user.name }}
          </div>
          <a href="#" class="fw-semobold text-muted text-hover-primary fs-7">
            {{ user.email }}
          </a>
        </div>
      </div>
    </div>

    <div
      class="menu-item px-5"
      data-kt-menu-trigger="hover"
      data-kt-menu-placement="left-start"
      data-kt-menu-flip="center, top"
    >
      <router-link to="/profile" class="menu-link px-5">
        <span class="menu-title position-relative">
          {{ $t("fields.language") }}
          <span
            class="fs-8 rounded menu-special px-3 py-2 position-absolute translate-middle-y top-50 end-0 d-flex"
          >
            <span class="title-type">{{ currentLanguageLocale.name }}</span>
            <img
              class="w-15px h-15px rounded-1 ms-2"
              :src="currentLanguageLocale.flag"
              alt="MDM Pro"
            />
          </span>
        </span>
      </router-link>

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
              <img class="rounded-1" :src="lang.flag" alt="MDM Pro" />
            </span>
            {{ lang.name ?? "Name" }}
          </a>
        </div>
      </div>
    </div>

    <div class="menu-item px-5 my-1">
      <router-link to="/profile" class="menu-link px-5">
        {{ $t("title.accountSettings") }}
      </router-link>
    </div>

    <div class="menu-item px-5 my-1" v-if="$can('SuperAdmin') && false">
      <a href="#" class="menu-link px-5" @click.prevent="copyToken">
        Copy UserToken
      </a>
    </div>

    <div class="menu-item px-5">
      <router-link to="/sign-out" class="menu-link px-5">
        {{ $t("action.signOut") }}
      </router-link>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, computed, inject } from "vue";
import { useI18n } from "vue-i18n";
import { useStore } from "@/store";
import { useRouter } from "vue-router";
import { Actions } from "@/store/enums/StoreEnums";
import { WSSignalR } from "@/core/plugins/signalr";
import {
  ILanguage,
  LanguageCodes,
  LanguageTypes,
} from "@/core/types/LanguageTypes";

import Clipboard from "clipboard";
import JwtService from "@/core/services/JwtService";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
import { getTenantLanguagesOptions } from "@/core/types/TenantTypes";
export default defineComponent({
  name: "kt-user-menu",

  setup() {
    const router = useRouter();
    const i18n = useI18n();
    const store = useStore();
    const user = store.state.AuthModule.user;

    const wsSignalR = inject(TenantGlobalInjectionKeys.WS_SIGNAL_R);

    let storageLanguage = localStorage.getItem("language");
    i18n.locale.value =
      LanguageCodes.all.find((lang) => lang === storageLanguage) ||
      LanguageCodes.enUS;

    let countries: ILanguage[];
    countries = getTenantLanguagesOptions.value;
    const signOut = () => {
      wsSignalR?.disconnect();
      store
        .dispatch(Actions.LOGOUT)
        .then(() => router.push({ name: "sign-in" }));
    };

    const setLang = (language: string) => {
      store.dispatch(Actions.SET_LANG, { language: language });
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

    const copyToken = () => {
      Clipboard.copy(JwtService.getToken() as string);
    };

    return {
      signOut,
      setLang,
      currentLanguage,
      currentLanguageLocale: currentLanguageLocale,
      countries,
      user,
      copyToken,
    };
  },
});
</script>
<style scoped lang="scss">
.menu-column {
  width: max-content !important;
  min-width: 275px;
  & .menu-item {
    .menu-link {
      .menu-title {
        color: #081735;
        .title-type {
          color: #868d98;
        }
      }
      color: #081735;
      &::after {
        content: none !important;
        display: none !important;
      }
    }
  }
}
.menu-special {
  background-color: #f2f4f7;
  color: #868d98;
}
</style>
