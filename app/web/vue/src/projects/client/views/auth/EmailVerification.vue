<template>
  <div class="d-flex flex-column flex-center flex-column-fluid">
    <!--begin::Content-->
    <div class="d-flex flex-column flex-center text-center p-10">
      <!--begin::Wrapper-->
      <div class="card card-flush w-lg-650px py-5">
        <div class="card-body py-15 py-lg-20">
          <!--begin::Title-->
          <h1 class="fw-bolder fs-2hx text-gray-900 mb-4">
            {{ $t("tip.emailVerification") }}
          </h1>
          <!--end::Title-->
          <!--begin::Text-->
          <div class="fw-semibold fs-6 text-gray-500 mb-7">
            {{ $t("tip.EmailVerificationText") }}
          </div>
          <!--end::Text-->
          <!--begin::Illustration-->
          <div class="mb-3">
            <img
              src="assets/media/illustrations/verification.svg"
              alt="verification"
              class="mw-100 mh-250px"
            />
          </div>
          <!--end::Illustration-->
          <!--begin::Link-->
          <div class="mb-0">
            <router-link to="/" class="btn btn-sm btn-primary">{{
              $t("title.returnHome")
            }}</router-link>
          </div>
          <!--end::Link-->
        </div>
      </div>
      <!--end::Wrapper-->
    </div>
    <!--end::Content-->
  </div>
</template>

<script lang="ts" setup>
import { useRouter } from "vue-router";
import { onMounted } from "vue";
import { useStore } from "@/store";
import { Actions } from "@/store/enums/StoreEnums";
import ApiService from "@/core/services/ApiService";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const router = useRouter();
const store = useStore();
const verifyEmail = () => {
  ApiService.post("auth/verify-email", router.currentRoute.value.query)
    .then(() => store.dispatch(Actions.LOGOUT))
    .then(() => router.push({ name: "sign-in" }));
};
onMounted(() => {
  verifyEmail();
  store.dispatch(Actions.ADD_BODY_CLASSNAME, "bg-body");
  store.dispatch(Actions.REMOVE_BODY_CLASSNAME, "page-loading");
});
</script>
