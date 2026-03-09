<template>
  <!--begin::Authentication Layout -->
  <div class="d-flex flex-column flex-lg-row flex-column-fluid">
    <!--begin::Body-->
    <div
      class="d-flex flex-column flex-lg-row-fluid w-lg-50 p-10 order-2 order-lg-1"
    >
      <!--begin::form-->
      <div class="d-flex flex-center flex-column flex-lg-row-fluid">
        <!--begin::Wrapper-->
        <div class="<?php echo $params['wrapperClass']?> p-10">
          <router-view></router-view>
        </div>
        <!--end::Wrapper-->
      </div>
      <!--end::form-->

      <!--begin::Footer-->
      <div class="d-flex flex-center flex-wrap px-5"></div>
      <!--end::Footer-->
    </div>
    <!--end::Body-->

    <!--begin::Aside-->
    <div
      class="d-flex flex-lg-row-fluid w-lg-25 bgi-size-cover bgi-position-center order-1 order-lg-2"
      style="background-image: url('/images/bg/auth-screens.png')"
    >
      <!--begin::Content-->
      <div
        class="d-flex flex-column flex-center py-7 py-lg-15 px-5 px-md-15 w-100"
      >
        <!--begin::Logo-->
        <router-link to="/" class="mb-0 mb-lg-12 d-flex align-items-center">
          <img
            alt="Logo"
            src="/images/logos/logo@2x.png"
            class="h-60px h-lg-75px"
          />
          <span style="font-size: 50px; color: #ffff">MM</span>
        </router-link>
        <!--end::Logo-->

        <!--begin::Image-->
        <img
          class="d-none d-lg-block mx-auto w-275px w-md-50 w-xl-500px mb-10 mb-lg-20"
          src="/images/bg/auth-screens.png"
          alt=""
        />
        <!--end::Image-->
      </div>
      <!--end::Content-->
    </div>
    <!--end::Aside-->
  </div>
  <!--end::Authentication Layout -->
</template>

<script setup lang="ts">
import { onMounted, onUnmounted } from "vue";
import { useStore } from "@/store";
import { Actions } from "@/store/enums/StoreEnums";
import { useRouter } from "vue-router";

const store = useStore();
const router = useRouter();

onMounted(() => {
  if (store.getters.isUserAuthenticated) {
    // console.log("isUser2fa", store.getters.isUser2fa);
    if (!store.getters.isUser2fa) {
      router.push({ name: "2fa" });
    } else {
      router.push({ name: "dashboard" });
    }
  }
  store.dispatch(Actions.ADD_BODY_CLASSNAME, "bg-body");
});

onUnmounted(() => {
  store.dispatch(Actions.REMOVE_BODY_CLASSNAME, "bg-body");
});
</script>
