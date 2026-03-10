<template>
  <el-dialog
    v-model="dialogRef"
    :title="$t('action.addDeleteTags')"
    width="30%"
    align-center
  >
    <el-checkbox-group v-model="accountTags">
      <el-checkbox
        v-for="(item, index) in tagOptions"
        :key="index"
        :label="item.label"
        border
      >
      </el-checkbox>
    </el-checkbox-group>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="dialogRef = false">{{
          $t("action.cancel")
        }}</el-button>
        <el-button type="primary" @click="updateData">
          {{ $t("action.submit") }}
        </el-button>
      </span>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import type { FormInstance } from "element-plus";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import {
  AccountTagTypes,
  getAllAccountTagTypes,
} from "@/core/types/AccountTagTypes";
import { ITEM_RENDER_EVT } from "element-plus/es/components/virtual-list/src/defaults";

const props = defineProps({
  refresh: {
    type: Function,
    required: true,
  },
});
const isLoading = ref(true);
const dialogRef = ref(false);
const id = ref(0);

let tagOptions = ref(getAllAccountTagTypes());

const accountTags = ref([]);

const fetchData = async () => {
  isLoading.value = true;
  const res = await AccountService.getAccountById(id.value);
  accountTags.value = res.accountTags.map((item: any) => item.name);
  isLoading.value = false;
};

const updateData = async () => {
  isLoading.value = true;
  const finalData = {
    id: id.value,
    tagNames: accountTags.value,
  };
  try {
    await AccountService.updateAccountTagsById(id.value, finalData).then(
      (res) => {
        MsgPrompt.success("Update Success");
        props.refresh();
        hide();
      }
    );
  } catch (err) {
    MsgPrompt.error("Update Failed");
  }
  isLoading.value = false;
};

const show = async (item: any) => {
  id.value = item.id;
  console.log(id.value);
  fetchData();
  dialogRef.value = true;
};

const hide = () => {
  dialogRef.value = false;
};

defineExpose({
  show,
  hide,
});
</script>
