<template>
  <el-tabs v-model="tab" @click="tabClicked()">
    <el-tab-pane
      :label="$t('fields.unconfimredAccount')"
      name="unconfimredAccount"
      :disabled="isLoading"
    ></el-tab-pane>
    <el-tab-pane
      :label="$t('fields.confimredAccount')"
      name="confimredAccount"
      :disabled="isLoading"
    ></el-tab-pane>
  </el-tabs>
  <div class="card">
    <div class="card-body">
      <table class="table table-tenant">
        <thead>
          <tr>
            <th>{{ $t("action.action") }}</th>
            <th>{{ $t("fields.status") }}</th>
            <th>{{ $t("fields.id") }}</th>
            <th>{{ $t("fields.type") }}</th>
            <th>{{ $t("fields.currency") }}</th>
            <th>{{ $t("fields.wallet") + " ID" }}</th>
            <th>MT</th>
            <th>{{ $t("fields.tradeAccount") }}</th>
            <th>{{ $t("fields.name") }}</th>
            <th>{{ $t("fields.email") }}</th>
            <th>{{ $t("fields.site") }}</th>
            <th>{{ $t("fields.group") }}</th>
            <th>{{ $t("fields.role") }}</th>
            <th>{{ $t("fields.sales") }}</th>
            <th>{{ $t("fields.ib") }}</th>
            <th>{{ $t("fields.created_at") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && data.length === 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr
            v-for="(item, index) in data"
            :key="index"
            :class="{
              'text-gray-600': item.status === 1,
              'text-gray-400': item.status === 2,
              'account-select': item.id === accountSelected,
            }"
            @click="selectedAccount(item.id)"
          >
            <td>
              <el-dropdown>
                <el-button type="primary" plain>
                  {{ $t("action.action")
                  }}<el-icon class="el-icon--right"><arrow-down /></el-icon>
                </el-button>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item
                      v-if="tab == 'unconfimredAccount'"
                      @click="showConfirmModal(item)"
                      >{{ $t("action.confirm") }}</el-dropdown-item
                    >
                    <el-dropdown-item
                      divided
                      @click="handleOpenUserDetail(item.user?.partyId)"
                    >
                      {{ $t("title.userDetails") }}
                    </el-dropdown-item>

                    <el-dropdown-item
                      divided
                      @click="showAccountDetail(item.id, 'infos')"
                    >
                      {{ $t("title.accountDetails") }}
                    </el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </td>

            <td @click="handleGodModeClick(item.partyId)">
              <AccountStatusBadge :status="item.status" :showDot="true" />
              <i
                v-if="item.accountTags.includes('Test')"
                class="fa-solid fa-vial text-danger m-1"
              ></i>
              <i
                v-if="item.accountTags.includes('DailyConfirmEmail')"
                class="fa-regular fa-file text-primary m-1"
              ></i>
            </td>

            <td :title="`${item.id} (${item.partyId})`">
              {{ item.uid
              }}<TinyCopyBox
                class="p-1"
                :val="item.uid.toString()"
              ></TinyCopyBox>
            </td>
            <td>{{ $t(`type.shortAccount.${item.type}`) }}</td>
            <td>
              {{ $t(`type.currency.${item.currencyId}`) }}
            </td>
            <td>
              {{ item.walletId }}
            </td>
            <td>
              {{ item.tradeAccount.serviceName }}
            </td>
            <td>
              <span
                role="button"
                @click="
                  viewComments(CommentType.Account, item.id, item.accountNumber)
                "
              >
                {{ item.accountNumber }}
                <i
                  v-if="item.hasComment"
                  class="fa-regular fa-comment-dots text-primary"
                ></i>
              </span>
              <TinyCopyBox
                v-if="item.accountNumber"
                :val="item.accountNumber.toString()"
              ></TinyCopyBox>
            </td>
            <td>
              <span
                role="button"
                @click="
                  viewComments(
                    CommentType.User,
                    item.user.partyId,
                    item.user.firstName + ' ' + item.user.lastName
                  )
                "
              >
                <div class="d-flex align-items-center gap-1">
                  {{ item.user.firstName }} {{ item.user.lastName }}
                  <i
                    v-if="item.user.hasComment"
                    class="fa-regular fa-comment-dots text-primary"
                  ></i>
                  <i
                    v-if="item.user.status == 2"
                    class="fa-solid fa-circle-dot text-danger"
                  ></i>
                  <el-icon v-if="item.isInIpBlackList" color="#FF0000"
                    ><WarningFilled
                  /></el-icon>
                  <el-icon v-if="item.isInUserBlackList"><Avatar /></el-icon>
                </div>
              </span>
            </td>
            <td>
              {{ item.user.email
              }}<TinyCopyBox class="p-1" :val="item.user.email"></TinyCopyBox>
            </td>
            <td>{{ $t(`type.siteType.${item.siteId}`) }}</td>
            <td>
              <div
                class="d-flex justify-content-start align-items-center gap-2"
              >
                <ViewReferPath :item="item" :paymentService="paymentService" />
                <div>{{ item.group }}</div>
              </div>
            </td>
            <td>
              {{ $t(`type.accountRole.${item.role}`) }}
            </td>

            <td>
              <template v-if="item.role === AccountRoleTypes.Sales">
                <span class="me-1"> {{ item.code }}</span>
                <TinyCopyBox :val="item.code.toString()"></TinyCopyBox>
              </template>
              <span v-else>
                <span
                  role="button"
                  @click="
                    viewComments(
                      CommentType.Account,
                      item.salesAccount.id,
                      item.salesAccount.user.firstName +
                        ' ' +
                        item.salesAccount.user.lastName
                    )
                  "
                >
                  [{{ item.salesAccount.code }}]
                  {{ item.salesAccount.uid }}

                  <i
                    v-if="item.salesAccount.hasComment"
                    class="fa-regular fa-comment-dots text-primary"
                  ></i>
                </span>
                <TinyCopyBox
                  :val="item.salesAccount.uid.toString()"
                ></TinyCopyBox>
              </span>
            </td>
            <td>
              <span v-if="item.agentAccount.id == 0">No IB</span>
              <span v-else>
                <span
                  role="button"
                  @click="
                    viewComments(
                      CommentType.Account,
                      item.agentAccount.id,
                      item.agentAccount.user.firstName +
                        ' ' +
                        item.agentAccount.user.lastName
                    )
                  "
                  >[{{ item.agentAccount.group }}]
                  {{ item.agentAccount.uid }}
                  <i
                    v-if="item.agentAccount.hasComment"
                    class="fa-regular fa-comment-dots text-primary"
                  ></i
                ></span>
                <TinyCopyBox
                  :val="item.agentAccount.uid.toString()"
                ></TinyCopyBox>
              </span>
            </td>
            <td><TimeShow :date-iso-string="item.createdOn" /></td>
          </tr>
        </tbody>
      </table>
    </div>
    <CommentsView ref="commentsRef" type="" id="0" />
    <AccountDetail ref="accountDetailRef" />
  </div>
</template>
<script lang="ts" setup>
import { ref, onMounted, inject, provide } from "vue";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import UserService from "@/projects/tenant/modules/users/services/UserService";
import { CommentType } from "@/core/types/CommentType";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import TinyCopyBox from "@/components/TinyCopyBox.vue";
import CommentsView from "@/projects/tenant/components/CommentView.vue";
import { Avatar, WarningFilled } from "@element-plus/icons-vue";
import ViewReferPath from "@/projects/tenant/modules/accounts/components/modal/ViewReferPath.vue";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
import { ArrowDown } from "@element-plus/icons-vue";
import InjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import AccountDetail from "../components/AccountDetail.vue";
const paymentService = ref(Array<any>());
const isLoading = ref(false);
const data = ref<any[]>([]);
const commentsRef = ref<any>(null);
const tab = ref("unconfimredAccount");
const accountSelected = ref(0);
const accountDetailRef = ref<InstanceType<typeof AccountDetail>>();
const criteria = ref<any>({
  page: 1,
  size: 20,
  sortField: "CreatedOn",
  tagName: "AutoCreate",
});

const openConfirmModal = inject<any>(
  TenantGlobalInjectionKeys.OPEN_CONFIRM_MODAL
);

const showAccountDetail = (id: number, _tab = "infos") => {
  accountDetailRef.value.show(id, _tab, paymentService.value);
};

const handleOpenUserDetail =
  inject(InjectionKeys.OPEN_USER_DETAILS) ?? (() => null);

provide(AccountInjectionKeys.HANDLE_REFRESH_ACCOUNT_CLIENT_TABLE, () =>
  fetchData(criteria.value.page)
);

const tabClicked = async () => {
  if (tab.value === "unconfimredAccount") {
    criteria.value.tagName = "AutoCreate";
  } else {
    criteria.value.tagName = "AutoCreateConfirmed";
  }
  await fetchData(1);
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  try {
    const response = await AccountService.queryAccounts(criteria.value);
    criteria.value = response.criteria;
    data.value = response.data;
  } catch (error) {
    console.error(error);
  }

  isLoading.value = false;
};

const showConfirmModal = (item: any) => {
  openConfirmModal?.(() =>
    AccountService.confirmAutoCreateAccount(item.id).then(() => fetchData(1))
  );
};

const handleGodModeClick = UserService.generateGodModeHandler();

const viewComments = (type: CommentType, id: number, title: string) => {
  commentsRef.value?.show(type, id, title);
};

const selectedAccount = (id: number) => {
  accountSelected.value = id;
};

onMounted(() => {
  fetchData(1);
});
</script>
<style scoped>
.confirm-box .el-message-box_message {
  font-size: 16px;
}
</style>
