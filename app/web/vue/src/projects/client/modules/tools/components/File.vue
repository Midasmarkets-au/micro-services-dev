<template>
  <div class="col-12 col-sm-12 col-xl">
    <!--begin::Card-->
    <div class="card h-100">
      <!--begin::Card body-->
      <div
        class="card-body d-flex justify-content-center text-center flex-column p-8"
      >
        <!--begin::Name-->
        <div class="text-gray-800 d-flex flex-column">
          <!--begin::Image-->
          <div class="symbol symbol-60px mb-6">
            <img :src="`/images/download/${fileType}.png`" alt="" />
          </div>
          <!--end::Image-->

          <!--begin::Title-->
          <div class="fs-5 fw-bold mb-2">
            {{ fileTitle }}
          </div>
          <!--end::Title-->
        </div>
        <!--end::Name-->

        <!--begin::Description-->
        <a
          herf="#"
          class="fs-7 fw-semobold text-gray-400 text-hover-primary mt-auto download-btn"
        >
          {{ createdAt }}
        </a>
        <!--end::Description-->
      </div>
      <!--end::Card body-->
    </div>
    <!--end::Card-->
  </div>
</template>

<script lang="ts">
import { defineComponent } from "vue";

export default defineComponent({
  name: "file-card",
  props: {
    fileTitle: String,

    createdAt: String,

    fileType: String,
  },
  components: {},
});
</script>

<style>
.download-btn:hover {
  cursor: pointer;
}
</style>
