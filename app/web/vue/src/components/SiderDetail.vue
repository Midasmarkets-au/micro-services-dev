<template>
  <div
    :id="props.elId"
    class="bg-body"
    data-kt-drawer="true"
    :data-kt-drawer-name="props.elId"
    data-kt-drawer-activate="true"
    data-kt-drawer-overlay="true"
    :data-kt-drawer-width="
      props.width ? props.width : '{default:\'80%\', \'md\': \'60%\'}'
    "
    data-kt-drawer-direction="start"
    :data-kt-drawer-toggle="'#' + props.elId + '_toggle'"
    data-kt-drawer-close="#sider_detail_close"
    ref="siderDetailRef"
  >
    <!--begin::Card-->
    <div class="card shadow-none rounded-0 w-100">
      <!--begin::Header-->
      <div class="card-header">
        <h5 class="card-title fw-semobold text-gray-600 position-relative pe-6">
          {{ props.title }}
          <span
            class="position-absolute top-50 end-0 translate-middle-y lh-0 me-1"
            v-if="props.isLoading"
          >
            <span
              class="spinner-border h-15px w-15px align-middle text-gray-400"
            ></span>
          </span>
        </h5>
      </div>
      <div class="card-body">
        <slot></slot>
      </div>
      <div class="card-footer" v-if="showFooter">
        <button
          id="sider_detail_close"
          class="btn btn-light me-3"
          type="reset"
          @click="props.discard"
        >
          {{ props.discardTitle ? props.discardTitle : $t("action.discard") }}
        </button>
        <button
          id="role_show_submit"
          class="btn btn-primary"
          type="submit"
          :disabled="props.submited || props.isLoading || props.isDisabled"
          @click="props.save"
        >
          <span v-if="props.submited">
            {{ props.savingTitle ? props.savingTitle : $t("action.saving") }}
            <span
              class="spinner-border h-15px w-15px align-middle text-gray-400"
            ></span>
          </span>
          <span v-else>{{
            props.saveTitle ? props.saveTitle : $t("action.save")
          }}</span>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { DrawerComponent } from "@/assets/ts/components";
import { ref, onMounted } from "vue";

const props = defineProps({
  title: { type: String, required: true },
  submited: { type: Boolean, required: true },
  isLoading: { type: Boolean, required: true },
  save: { type: Function, required: true },
  saveTitle: String,
  savingTitle: String,
  discardTitle: String,
  discard: { type: Function, required: true },
  elId: { type: String, required: true },
  width: String,
  isDisabled: Boolean,
  showFooter: { type: Boolean, required: false, default: true },
});

const siderDetailRef = ref<Node>();

onMounted(() => {
  document.body.appendChild(siderDetailRef.value as Node);
});

const show = () => {
  // console.log("has: ", DrawerComponent.hasInstace(props.elId));
  let d = null;
  if (DrawerComponent.hasInstace(props.elId)) {
    d = DrawerComponent.getInstance(props.elId);
  } else {
    d = DrawerComponent.createInstances(props.elId);
  }
  d.show();
};

const hide = () => {
  const d = DrawerComponent.getInstance(props.elId);
  d.hide();
};
defineExpose({
  hide,
  show,
});
</script>
