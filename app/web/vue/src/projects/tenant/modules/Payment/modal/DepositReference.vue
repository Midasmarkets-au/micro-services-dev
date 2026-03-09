<template>
  <div>
    <SimpleForm
      ref="modalForm"
      :title="$t('title.poliReferanceNumber')"
      :is-loading="isLoading"
      disable-footer
    >
      <div class="fs-3">
        {{ $t("title.referanceNumber") }}: <strong>{{ referNumber }}</strong>
      </div>
    </SimpleForm>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";

const isLoading = ref(true);
const modalForm = ref<InstanceType<typeof SimpleForm>>();
const referNumber = ref("");

defineExpose({
  async show(_referNumber: any) {
    modalForm.value?.show();
    referNumber.value = _referNumber.TransactionRefNo;
  },
  hide() {
    modalForm.value?.hide();
  },
});
</script>

<style scoped></style>
