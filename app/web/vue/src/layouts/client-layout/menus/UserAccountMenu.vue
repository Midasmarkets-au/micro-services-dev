<template>
  <!--begin::Menu-->
  <div
    class="menu menu-sub menu-sub-dropdown menu-column menu-rounded menu-gray-600 menu-state-bg-light-primary fw-semobold py-4 fs-6 w-275px"
    data-kt-menu="true"
  >
    <!--begin::Menu item-->
    <div class="menu-item px-3">
      <div class="menu-content d-flex align-items-center px-3">
        <!--begin::Avatar-->
        <UserAvatar
          :avatar="user?.avatar"
          :alt="user?.name"
          side="client"
          size="50px"
          v-if="user"
        />
        <!--end::Avatar-->

        <!--begin::Username-->
        <div class="d-flex flex-column">
          <div class="fw-bold d-flex align-items-center fs-5">
            {{ user.name }}
            <UserRole :role="user.roles" />
          </div>
          <a href="#" class="fw-semobold text-muted text-hover-primary fs-7">
            {{ user.email }}
          </a>
        </div>
        <!--end::Username-->
      </div>
    </div>
    <!--end::Menu item-->

    <!--begin::Menu item-->
    <div
      class="menu-item px-5"
      data-kt-menu-trigger="hover"
      data-kt-menu-placement="left-start"
      data-kt-menu-flip="center, top"
    >
      <div class="menu-link px-5">
        <span class="menu-title position-relative">
          Language
          <span
            class="fs-8 rounded bg-light px-3 py-2 position-absolute translate-middle-y top-50 end-0"
          >
            {{ currentLanguageLocale.name }}
            <img
              class="w-15px h-15px rounded-1 ms-2"
              :src="currentLanguageLocale.flag"
              alt="MM Pro"
            />
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
            {{ lang.name }}
          </a>
        </div>
        <!--end::Menu item-->
      </div>
      <!--end::Menu sub-->
    </div>
    <!--end::Menu item-->

    <!--begin::Menu item-->
    <div class="menu-item px-5 my-1">
      <router-link to="/profile" class="menu-link px-5">
        Account Settings
      </router-link>
    </div>
    <!--end::Menu item-->

    <!--begin::Menu item-->
    <div class="menu-item px-5 my-1">
      <router-link to="/profile" class="menu-link px-5">
        Account Settings
      </router-link>
    </div>
    <!--end::Menu item-->

    <!--begin::Menu item-->
    <div class="menu-item px-5">
      <a @click="signOut()" class="menu-link px-5"> Sign Out</a>
    </div>
    <!--end::Menu item-->
  </div>
  <!--end::Menu-->
</template>

<script lang="ts">
import { defineComponent, computed } from "vue";
import { useI18n } from "vue-i18n";
import { useStore } from "@/store";
import { useRouter } from "vue-router";
import { Actions } from "@/store/enums/StoreEnums";
import UserRole from "@/components/UserRole.vue";
import {
  ILanguage,
  LanguageCodes,
  LanguageTypes,
} from "@/core/types/LanguageTypes";
import { string } from "yup";

export default defineComponent({
  name: "kt-user-menu",
  methods: {
    string() {
      return string;
    },
  },
  components: {
    UserRole,
  },
  setup() {
    const router = useRouter();
    const i18n = useI18n();
    const store = useStore();
    const user = store.state.AuthModule.user;
    let storageLanguage = localStorage.getItem("language");
    i18n.locale.value =
      LanguageCodes.all.find((lang) => lang === storageLanguage) ||
      LanguageCodes.enUS;

    let countries: ILanguage[] = LanguageTypes.all;

    const signOut = () => {
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

    return {
      signOut,
      setLang,
      currentLanguage,
      currentLanguageLocale: currentLanguageLocale,
      countries,
      user,
    };
  },
});
</script>
