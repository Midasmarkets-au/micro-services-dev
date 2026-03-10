<template>
  <el-dialog v-model="dialogRef" title="Move" width="500" align-center>
    <div class="fs-5 mb-8">
      Current Tenant: <b>{{ user.tenancy }}</b>
    </div>
    <el-form>
      <el-form-item label="Move to" prop="tenant">
        <el-select
          v-model="tenantDestination"
          placeholder="Select"
          :disabled="isLoading"
        >
          <el-option
            v-for="item in tenantOptions"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          ></el-option>
        </el-select>
      </el-form-item>
    </el-form>
    <div>
      <div v-for="(item, index) in afterMoveData" :key="index">
        <div>{{ item }}</div>
      </div>
    </div>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="hide()" :disabled="isLoading">{{
          $t("action.cancel")
        }}</el-button>
        <el-button
          v-if="!hideSubmit"
          type="primary"
          @click="submit()"
          :disabled="tenantDestination == null"
          :loading="isLoading"
        >
          {{ $t("action.submit") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref, nextTick, inject } from "vue";
import {
  TenantOptions,
  getUserTenancy,
  TenantTypes,
} from "@/core/types/TenantTypes";
import UserService from "@/projects/tenant/modules/users/services/UserService";

const dialogRef = ref(false);
const partyId = ref<any>(0);
const tenantOptions = ref<any>([]);
const tenantDestination = ref<any>(null);
const isLoading = ref(false);
const user = ref<any>(null);
const afterMoveData = ref<any>(null);
const fecthData = inject<any>("fetchData");
const hideSubmit = ref(false);
const show = async (_partyId: number, _user) => {
  dialogRef.value = true;
  partyId.value = _partyId;
  user.value = _user;
  await nextTick();
  await removeOwnTenancy(_user);
};
const emits = defineEmits<{
  (e: "user-moved"): void;
}>();
const submit = async () => {
  isLoading.value = true;
  try {
    const res = await UserService.migrateUnverifiedUser(
      partyId.value,
      tenantDestination.value
    );
    afterMoveData.value = res;
    fecthData(1);
    hideSubmit.value = true;
    // dialogRef.value = false;
    // emits("user-moved");
  } catch (e) {
    console.log(e);
  }
  isLoading.value = false;
};

const removeOwnTenancy = async (user) => {
  const tenancy = user?.tenancy;
  tenantOptions.value = TenantOptions.filter(
    (option: any) => option.value !== TenantTypes[tenancy]
  );
};

const hide = () => {
  dialogRef.value = false;
  user.value = null;
  tenantDestination.value = null;
  isLoading.value = false;
  afterMoveData.value = null;
  hideSubmit.value = false;
};

defineExpose({
  show,
  hide,
});
</script>
