<template>
  <el-popover trigger="click" placement="left" :width="900" @show="onShow">
    <template #default>
      <div v-if="isShow">
        <table class="table table-row-dashed">
          <thead>
            <tr class="text-muted fw-bold text-uppercase fs-7">
              <th>Lv.</th>
              <th>{{ $t("fields.name") }}</th>
              <th>{{ $t("fields.uid") }} / {{ $t("fields.accountNo") }}</th>
              <th>{{ $t("fields.email") }}</th>
              <th>{{ $t("fields.role") }}</th>
              <th>{{ $t("fields.group") }}/ {{ $t("fields.code") }}</th>
              <th>{{ $t("action.action") }}</th>
            </tr>
          </thead>
          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && data.length === 0">
            <NoDataBox />
          </tbody>
          <tbody v-else>
            <tr v-for="(item, index) in data" :key="index">
              <td>Lv. {{ index + 1 }}</td>
              <td @click="handleGodModeClick(item.partyId)">{{ item.name }}</td>
              <td>{{ item.uid }} / {{ item.accountNumber }}</td>
              <td>{{ item.user.email }}</td>
              <td>{{ $t(`type.accountRole.${item.role}`) }}</td>
              <td>
                <!-- <div
                  v-if="item.role === UserRoleTypes.IB"
                  class="d-flex align-items-center justify-content-between"
                >
                  <div>{{ item.group }}</div>
                  <div>
                    <el-button size="small" @click="showChangeGroup(item)">{{
                      $t("action.edit")
                    }}</el-button>
                  </div>
                </div> -->
                <div v-if="item.role === UserRoleTypes.IB">
                  <el-button
                    size="small"
                    :icon="Edit"
                    @click="showChangeGroup(item.id, item.group)"
                    >{{ item.group }}</el-button
                  >
                </div>
                <div v-if="item.role === UserRoleTypes.Sales">
                  {{ item.code }}
                </div>
              </td>
              <td>
                <el-button
                  size="small"
                  type="success"
                  @click="showAccountDetail(item.id)"
                  >{{ $t("action.view") }}</el-button
                >
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <ChangeGroupForm ref="changeGroupRef" />
    </template>
    <template #reference>
      <el-button :icon="Memo" size="small" circle></el-button>
    </template>
  </el-popover>
  <AccountDetail ref="accountDetailRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import AccountService from "../../services/AccountService";
import AccountDetail from "../AccountDetail.vue";
import { Memo, Edit } from "@element-plus/icons-vue";
import { UserRoleTypes } from "@/core/types/RoleTypes";
import ChangeGroupForm from "../form/ChangeGroupForm.vue";
import UserService from "@/projects/tenant/modules/users/services/UserService";
const props = defineProps<{
  item: any;
  paymentService?: any;
}>();
const isLoading = ref(false);
const data = ref(<any>[]);
const isShow = ref(false);
const accountDetailRef = ref<InstanceType<typeof AccountDetail>>();
const changeGroupRef = ref<InstanceType<typeof ChangeGroupForm>>();
const fecthData = async () => {
  isLoading.value = true;
  try {
    const res = await AccountService.queryParentAccounts(props.item.id);
    data.value = res;
  } catch (e) {
    console.log(e);
  }
  isLoading.value = false;
};

const onShow = () => {
  isShow.value = true;
  fecthData();
};

const handleGodModeClick = UserService.generateGodModeHandler();

const showChangeGroup = (id: number, group: string) => {
  changeGroupRef.value?.show(id, group);
};
const showAccountDetail = (id: number) => {
  accountDetailRef.value?.show(id, "infos", [] as any);
};
</script>
