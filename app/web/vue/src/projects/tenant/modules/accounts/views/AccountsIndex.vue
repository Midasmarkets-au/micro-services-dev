<template>
  <!-- <div v-if="!isLoading" class="mb-4">
      <div class="btn btn-primary">{{ content }}</div>
      <Editor
        v-model="content"
        api-key="dgpx6ul0v883nikc809en1axx8z8rsjkha2fsobcyki3ad3n"
        :init="editorInitOptions"
      />
    </div> -->
  <!-- <OpenProcedureForm class="mb-10" /> -->
  <!-- <div class="mb-10 p-10">
      <input :placeholder="$t('tip.pleaseInput')" id="inputValue" />
      <div id="mytest"></div>
      
      <button :onclick="cc">Calculate</button>
    </div> -->
  <div class="card mb-xl-8">
    <div class="card-header">
      <div class="card-title">{{ $t("title.accounts") }}</div>
      <div class="card-toolbar">
        <!-- <el-input
          v-model="criteria.partyId"
          class="w-100 m-2"
          placeholder="Please Input Party ID"
          :suffix-icon="Search"
          @keyup.enter="fetchData(1)"
        /> -->
      </div>
    </div>
    <div class="card-body">
      <table
        class="table align-middle table-row-dashed fs-6 gy-1 table-striped table-hover"
        id="table_accounts_requests"
      >
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th class=""></th>
            <th class="">{{ $t("fields.status") }}</th>
            <th class="">{{ $t("fields.id") }}</th>
            <th class="">{{ $t("fields.type") }}</th>
            <th class="">{{ $t("fields.currency") }}</th>
            <th class="">{{ $t("fields.tradeAccount") }}</th>
            <th class="">{{ $t("fields.name") }}</th>
            <th class="">{{ $t("fields.group") }}</th>
            <th class="">{{ $t("fields.role") }}</th>
            <!-- <th class="">{{ $t("fields.platform") }}</th> -->
            <th class="">{{ $t("fields.server") }}</th>
            <th class="">{{ $t("fields.sales") }}</th>
            <th class="">{{ $t("fields.ib") }}</th>
            <th class="">{{ $t("fields.accountCode") }}</th>
            <th class="">{{ $t("title.rebate") }}</th>
            <th class="">{{ $t("fields.created_at") }}</th>
          </tr>
        </thead>

        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && liveAccounts.length === 0">
          <NoDataBox />
        </tbody>

        <tbody v-else class="fw-semibold text-gray-900">
          <tr v-for="(item, index) in liveAccounts" :key="index">
            <td>
              <el-dropdown trigger="click">
                <el-button type="primary" class="btn btn-secondary btn-sm">
                  {{ $t("action.action")
                  }}<el-icon class="el-icon--right"><arrow-down /></el-icon>
                </el-button>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item
                      @click="
                        openProcedureFormRef?.show(3, {
                          accountId: item.id,
                        })
                      "
                    >
                      {{ $t("tip.accountOpenProcedure") }}
                    </el-dropdown-item>
                    <el-dropdown-item
                      @click="showAccountDetail(item.id, 'infos')"
                    >
                      {{ $t("title.details") }}
                    </el-dropdown-item>
                    <el-dropdown-item
                      @click="showAccountDetail(item.id, 'trade')"
                    >
                      {{ $t("title.tradeHistory") }}
                    </el-dropdown-item>
                    <el-dropdown-item
                      @click="showAccountDetail(item.id, 'transaction')"
                    >
                      {{ $t("title.transferHistory") }}
                    </el-dropdown-item>
                    <el-dropdown-item
                      v-if="item.role == 400"
                      @click="showAccountDetail(item.id, 'rebate')"
                    >
                      {{ $t("title.rebate") }}
                    </el-dropdown-item>
                    <el-dropdown-item
                      @click="showAccountDetail(item.id, 'documents')"
                    >
                      {{ $t("title.documents") }}
                    </el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </td>
            <!--  -->
            <td><AccountStatusBadge :status="item.status" /></td>
            <td>
              <span
                role="button"
                @click="
                  viewComments(
                    CommentType.Account,
                    item.id,
                    item.user.firstName +
                      '' +
                      item.user.lastName +
                      '-' +
                      item.id
                  )
                "
              >
                {{ item.uid }}
                <i
                  v-if="item.hasComment"
                  class="fa-regular fa-comment-dots text-primary"
                ></i>
              </span>
            </td>

            <td>{{ $t(`type.account.${item.type}`) }}</td>
            <td>
              {{ t(`type.currency.${item.tradeAccount?.currencyId}`) }}
            </td>
            <td>{{ item.tradeAccount?.accountNumber }}</td>
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
                {{ item.user.firstName }} {{ item.user.lastName }}
                <i
                  v-if="item.user.hasComment"
                  class="fa-regular fa-comment-dots text-primary"
                ></i>
              </span>
            </td>
            <td>{{ item.group }}</td>
            <td>{{ $t(`type.accountRole.${item.role}`) }}</td>
            <!-- <td>
              {{ serviceMap[item.serviceId]?.platformName }}
            </td> -->
            <td>
              {{ serviceMap[item.serviceId]?.serverName }}
            </td>
            <td>
              <span v-if="item.salesAccount.id == 0">No Sales</span>
              <span
                v-else
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
                [{{ item.salesAccount.user.firstName }}
                {{ item.salesAccount.user.lastName }}]
                {{ item.salesAccount.uid }}

                <i
                  v-if="item.salesAccount.hasComment"
                  class="fa-regular fa-comment-dots text-primary"
                ></i>
              </span>
            </td>
            <td>
              <span v-if="item.agentAccount.id == 0">No IB</span>
              <span
                v-else
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
                >[{{ item.agentAccount.user.firstName }}
                {{ item.agentAccount.user.lastName }}]
                {{ item.agentAccount.uid }}
                <i
                  v-if="item.agentAccount.hasComment"
                  class="fa-regular fa-comment-dots text-primary"
                ></i
              ></span>
            </td>
            <td>{{ item.code }}</td>
            <td>{{ item.agentRebateRule }}</td>
            <td><TimeShow :date-iso-string="item.createdOn" /></td>
          </tr>
        </tbody>
      </table>

      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
    <AccountDetail ref="accountDetailRef" />
    <OpenProcedureForm ref="openProcedureFormRef" />

    <CommentsView ref="commentsRef" type="" id="0" />
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, nextTick } from "vue";
import AccountService from "../services/AccountService";
import OpenProcedureForm from "../components/OpenProcedureForm.vue";
import TimeShow from "@/components/TimeShow.vue";
import i18n from "@/core/plugins/i18n";
import TableFooter from "@/components/TableFooter.vue";
import AccountDetail from "../components/AccountDetail.vue";
import GlobalService from "@/projects/client/services/ClientGlobalService";
import { ServiceMapType } from "@/core/types/ServiceTypes";
import { reinitializeComponents } from "@/core/plugins/plugins";
import { ArrowDown } from "@element-plus/icons-vue";
// import { Search } from "@element-plus/icons-vue";

import CommentsView from "../../../components/CommentView.vue";
import { CommentType } from "@/core/types/CommentType";

const { t } = i18n.global;

const openProcedureFormRef = ref<InstanceType<typeof OpenProcedureForm>>();
const accountDetailRef = ref<any>(null);
const commentsRef = ref<InstanceType<typeof CommentsView>>();

const isLoading = ref(true);
const liveAccounts = ref(Array<any>());
const serviceMap = ref<ServiceMapType>({} as ServiceMapType);

const criteria = ref<any>({
  page: 1,
  size: 20,
  numPages: 1,
  total: 0,
  hasTradeAccount: true,
  sortField: "CreatedOn",
});

const paymentServiceCriteria = ref({
  size: 100,
});

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const responseBody = await AccountService.queryAccounts(criteria.value);
    criteria.value = responseBody.criteria;
    liveAccounts.value = responseBody.data;
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
    await nextTick();
    reinitializeComponents();
  }
};

onMounted(async () => {
  isLoading.value = true;
  serviceMap.value = await GlobalService.getServiceMap();
  await fetchData(1);
  isLoading.value = false;
});

const viewComments = (type: CommentType, id: number, title: string) => {
  commentsRef.value?.show(type, id, title);
};

const showAccountDetail = (id: number, page: string) => {
  accountDetailRef.value.show(id, page, [] as any);
};
</script>
