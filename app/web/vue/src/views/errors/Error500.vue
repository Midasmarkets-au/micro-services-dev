<template>
  <div class="d-flex flex-column flex-center flex-column-fluid">
    <!--begin::Content-->
    <div class="d-flex flex-column flex-center text-center p-10">
      <!--begin::Wrapper-->
      <div class="card card-flush w-lg-650px py-5">
        <div class="card-body py-15 py-lg-20">
          <!--begin::Title-->
          <h1 class="fw-bolder fs-2qx text-gray-900 mb-4">System Error</h1>
          <!--end::Title-->
          <!--begin::Text-->
          <div class="fw-semibold fs-6 text-gray-500 mb-7">
            Something went wrong! Please try again later.
          </div>
          <!--end::Text-->
          <!--begin::Illustration-->
          <div class="mb-11">
            <img
              src="/images/errors/500-error.png"
              class="mw-100 mh-300px theme-light-show"
              alt=""
            />
            <img
              src="/images/errors/500-error-dark.png"
              class="mw-100 mh-300px theme-dark-show"
              alt=""
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

<script lang="ts">
import { defineComponent } from "vue";
import { getIllustrationsPath } from "@/core/helpers/assets";

export default defineComponent({
  name: "error-500",
  components: {},
  setup() {
    return {
      getIllustrationsPath,
    };
  },
});
</script>
