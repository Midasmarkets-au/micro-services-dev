<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.details')"
    :is-loading="isLoading"
    :width="900"
    disable-footer
  >
    <el-descriptions :column="2" border>
      <el-descriptions-item :label="$t('fields.id')">
        {{ contact.id }}
      </el-descriptions-item>
      <el-descriptions-item :label="$t('fields.status')">
        <el-tag :type="contact.isArchived === 1 ? 'info' : 'success'">
          {{
            contact.isArchived === 1
              ? $t("status.archived")
              : $t("status.active")
          }}
        </el-tag>
      </el-descriptions-item>
      <el-descriptions-item :label="$t('fields.name')">
        {{ contact.name }}
      </el-descriptions-item>
      <el-descriptions-item :label="$t('fields.email')">
        {{ contact.email }}
      </el-descriptions-item>
      <el-descriptions-item :label="$t('fields.phone')">
        {{ contact.phoneNumber }}
      </el-descriptions-item>
      <el-descriptions-item :label="$t('fields.ip')">
        {{ contact.ip }}
      </el-descriptions-item>
      <el-descriptions-item :label="$t('fields.createdOn')">
        <TimeShow :date-iso-string="contact.createdOn" />
      </el-descriptions-item>
      <el-descriptions-item :label="$t('fields.partyId')">
        {{ contact.partyId }}
      </el-descriptions-item>
      <el-descriptions-item :label="$t('fields.content')" :span="2">
        <div class="white-space-pre-wrap">{{ contact.content }}</div>
      </el-descriptions-item>
    </el-descriptions>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import ContactService from "../services/ContactService";

const modalRef = ref<any>();
const isLoading = ref(false);
const contact = ref<any>({});

const show = async (_contact: any) => {
  contact.value = _contact;
  modalRef.value?.show();
  isLoading.value = true;
  try {
    contact.value = await ContactService.getContact(_contact.id);
  } catch (e) {
    MsgPrompt.error(e);
  } finally {
    isLoading.value = false;
  }
};

defineExpose({
  show,
  hide: () => modalRef.value?.hide(),
});
</script>

<style scoped>
.white-space-pre-wrap {
  white-space: pre-wrap;
}
</style>
