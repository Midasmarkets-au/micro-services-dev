<template>
  <div>
    <SimpleForm
      ref="modalForm"
      :title="$t('fields.changeUserSiteId')"
      :is-loading="isLoading"
      disable-footer
    >
      <div class="d-flex flex-column mb-9 fv-row">
        <Field
          name="serviceCurrencyId"
          class="form-select form-select-solid"
          as="select"
          v-model="siteId"
        >
          <option
            v-for="site in ConfigSiteTypesSelections"
            :key="site.value"
            :value="site.value"
            :label="site.label"
          ></option>
        </Field>
      </div>

      <div class="d-flex justify-content-center">
        <button
          class="btn btn-light btn-success btn-md me-3"
          @click="updateSiteId"
        >
          <span v-if="isLoading">
            {{ $t("action.waiting") }}
            <span
              class="spinner-border h-15px w-15px align-middle text-gray-400"
            ></span>
          </span>
          <span v-else>{{ $t("action.update") }}</span>
        </button>
      </div>
    </SimpleForm>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import { Field } from "vee-validate";
import UserService from "../../services/UserService";
import { ConfigSiteTypesSelections } from "@/core/types/SiteTypes";
import { ElNotification } from "element-plus";
const isLoading = ref(false);
const modalForm = ref<InstanceType<typeof SimpleForm>>();
const partyId = ref();
const siteId = ref();

const emits = defineEmits<{
  (e: "updateSiteID", siteId: number): void;
}>();

const updateSiteId = async () => {
  isLoading.value = true;
  try {
    await UserService.updateSiteID(partyId.value, siteId.value);
    ElNotification.success("Update Site ID successfully");
    emits("updateSiteID", siteId.value);
    modalForm.value?.hide();
  } catch (error) {
    ElNotification.error({
      title: "Error",
      message: error.message,
    });
  } finally {
    isLoading.value = false;
  }
};

defineExpose({
  async show(_partyId: any, _siteId: any) {
    modalForm.value?.show();
    partyId.value = _partyId;
    siteId.value = _siteId;
  },
  hide() {
    modalForm.value?.hide();
  },
});

onMounted(() => {
  // console.log(SiteTypes);
});
</script>

<style scoped></style>
