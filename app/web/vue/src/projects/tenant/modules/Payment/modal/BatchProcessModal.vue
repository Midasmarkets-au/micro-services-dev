<template>
  <el-dialog
    v-model="visible"
    :title="$t('title.batchProcessDetail')"
    width="95%"
    :close-on-click-modal="false"
  >
    <div class="table-responsive" style="max-height: 60vh; overflow: auto">
      <table
        class="table align-middle table-row-dashed fs-6 gy-5"
        id="table_batch_process"
      >
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th class="sticky-col">
              <el-checkbox
                v-model="isAllSelected"
                :indeterminate="isIndeterminate"
                @change="(val: any) => handleSelectAll(val as boolean)"
              />
            </th>
            <!-- 提现信息 -->
            <th>{{ $t("fields.createdOn") }}</th>
            <th>{{ $t("fields.status") }}</th>
            <th>{{ $t("fields.refId") }}</th>
            <th class="highlight-col">{{ $t("fields.nativeName") }}</th>
            <th>{{ $t("fields.email") }}</th>
            <th>{{ $t("fields.accountNumber") }}</th>
            <th>{{ $t("fields.salesCode") }}</th>
            <th>{{ $t("fields.ibGroup") }}</th>
            <th class="highlight-col">{{ $t("fields.balance") }}</th>
            <th class="highlight-col">{{ $t("fields.withdrawAmount") }}</th>
            <th class="highlight-col">{{ $t("fields.exchangeRate") }}</th>
            <th class="highlight-col">{{ $t("fields.actualAmount") }}</th>
            <!-- 支付方式 -->
            <th class="highlight-col">{{ $t("fields.usdtWalletAddress") }}</th>
            <th>{{ $t("fields.method") }}</th>
            <th>{{ $t("fields.countryOfBank") }}</th>
            <th>{{ $t("fields.bankName") }}</th>
            <th>{{ $t("fields.branchName") }}</th>
            <th>{{ $t("fields.city") }}</th>
            <th>{{ $t("fields.state") }}</th>
            <th>{{ $t("fields.accountHolder") }}</th>
            <th>{{ $t("fields.accountNo") }}</th>
            <th>{{ $t("fields.confirmAccountNo") }}</th>
            <th>{{ $t("fields.bsb") }}</th>
            <th>{{ $t("fields.swiftCode") }}</th>
            <th class="sticky-col-right">{{ $t("action.action") }}</th>
          </tr>
        </thead>

        <tbody v-if="isLoading">
          <tr>
            <td :colspan="26" class="text-center py-10">
              <span class="spinner-border spinner-border-sm me-2"></span>
              Loading...
            </td>
          </tr>
        </tbody>
        <tbody v-else-if="!isLoading && detailData.length === 0">
          <tr>
            <td :colspan="26" class="text-center py-10 text-muted">
              No data available
            </td>
          </tr>
        </tbody>

        <tbody v-else class="fw-semibold text-gray-900">
          <tr v-for="(item, index) in detailData" :key="index">
            <td class="sticky-col">
              <el-checkbox
                v-model="item.selected"
                @change="handleSelectionChange"
              />
            </td>
            <!-- 提现信息 -->
            <td>
              <TimeShow :date-iso-string="item.createdOn" />
            </td>
            <td>
              {{ $t(`type.paymentStatus.${item.status}`) }}
            </td>
            <td>{{ item.refId }}</td>
            <td class="highlight-col">{{ item.nativeName }}</td>
            <td>{{ item.email }}</td>
            <td>{{ item.accountNumber }}</td>
            <td>{{ item.salesCode }}</td>
            <td>{{ item.ibGroup }}</td>
            <td class="highlight-col">
              <div>
                <BalanceShow
                  :currency-id="item.currencyId"
                  :balance="item.balanceInCents"
                />
              </div>
              <div
                v-if="item.accountType === TransactionAccountType.Wallet"
                class="badge badge-primary"
              >
                Wallet # {{ item.sourceDisplayNumber }}
              </div>
              <div
                v-if="item.accountType === TransactionAccountType.TradeAccount"
                class="badge badge-warning"
              >
                Account # {{ item.sourceDisplayNumber }}
              </div>
            </td>
            <td class="highlight-col">
              <BalanceShow
                :currency-id="item.fromCurrencyId"
                :balance="item.amount"
              />
            </td>
            <td class="highlight-col">{{ item.exchangeRate }}</td>
            <td class="highlight-col">
              <BalanceShow
                :currency-id="item.toCurrencyId"
                :balance="item.actualAmount"
              />
            </td>
            <!-- 支付方式 -->
            <td class="highlight-col">
              <CopyBox
                v-if="item.walletAddress"
                :val="item.walletAddress"
                :hasIcon="true"
              />
            </td>
            <td>{{ item.paymentMethod }}</td>
            <td>{{ item.countryOfBank }}</td>
            <td>{{ item.bankName }}</td>
            <td>{{ item.branchName }}</td>
            <td>{{ item.city }}</td>
            <td>{{ item.state }}</td>
            <td>{{ item.accountHolder }}</td>
            <td>{{ item.bankAccountNo }}</td>
            <td>{{ item.confirmAccountNo }}</td>
            <td>{{ item.bsb }}</td>
            <td>{{ item.swiftCode }}</td>
            <td class="sticky-col-right">
              <div class="d-flex gap-2">
                <el-button
                  type="primary"
                  size="small"
                  @click="copyItemInfo(item)"
                >
                  {{ $t("action.copy") }}
                </el-button>
                <el-button
                  v-if="item.walletAddress"
                  type="success"
                  size="small"
                  @click="showQrCodeModal(item)"
                >
                  {{ $t("action.confirmTransfer") }}
                </el-button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <template #footer>
      <div class="d-flex justify-content-between align-items-center">
        <span class="text-muted">
          {{ $t("tip.selectedCount") }}: {{ selectedIds.length }} /
          {{ detailData.length }}
        </span>
        <div>
          <el-button @click="visible = false">
            {{ $t("action.cancel") }}
          </el-button>
          <el-button
            type="success"
            :disabled="selectedIds.length === 0"
            :loading="isExecuting"
            @click="executeBatch"
          >
            {{ $t("action.startProcess") }} ({{ selectedIds.length }})
          </el-button>
        </div>
      </div>
    </template>
  </el-dialog>

  <!-- 二维码 Modal -->
  <el-dialog
    v-model="qrCodeVisible"
    :title="$t('action.confirmTransfer')"
    width="400px"
    :close-on-click-modal="false"
  >
    <div class="d-flex flex-column align-items-center justify-content-center">
      <div class="qr-code mt-3 d-flex justify-content-center">
        <div ref="qrCodeRef" class="qrcode"></div>
      </div>
      <div class="mt-4 text-center">
        <p class="fw-semibold">{{ $t("tip.scanQrCodeUsingIMToken") }}</p>
        <p class="text-muted small mt-2">{{ currentWalletAddress }}</p>
      </div>
    </div>
    <template #footer>
      <el-button @click="qrCodeVisible = false">
        {{ $t("action.close") }}
      </el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, inject, nextTick } from "vue";
import QRCode from "qrcodejs2";
import TimeShow from "@/components/TimeShow.vue";
import BalanceShow from "@/components/BalanceShow.vue";
import CopyBox from "@/components/CopyBox.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import PaymentService from "../services/PaymentService";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import phonesData from "@/core/data/phonesData";
import InjectKeys from "@/core/types/TenantGlobalInjectionKeys";
import { TransactionAccountType } from "@/core/types/StateInfos";
import { TransactionAccountTypes } from "@/core/types/AccountInfos";
import Clipboard from "clipboard";
import { useStore } from "@/store";
import { useI18n } from "vue-i18n";
import filters from "@/core/helpers/filters";

const store = useStore();
const user = store.state.AuthModule.user;
const language = user.language;
const { t } = useI18n();

const visible = ref(false);
const isLoading = ref(false);
const isExecuting = ref(false);
const detailData = ref<any[]>([]);
const selectedIds = ref<number[]>([]);
const isAllSelected = ref(false);
const isIndeterminate = ref(false);

// 二维码 Modal 相关
const qrCodeVisible = ref(false);
const qrCodeRef = ref<HTMLDivElement>();
const currentWalletAddress = ref("");

const openConfirmBox = inject<any>(InjectKeys.OPEN_CONFIRM_MODAL);

const emits = defineEmits<{
  (e: "success"): void;
}>();

// 显示 Modal 并加载数据
const show = async (selectedItems: any[]) => {
  visible.value = true;
  isLoading.value = true;
  detailData.value = [];

  try {
    // 循环获取每个选中项的详情数据
    for (const item of selectedItems) {
      const withdrawalInfo: any = await PaymentService.getWithdrawalInfosById(
        item.id
      );

      // 获取账户信息（与 WithdrawInfo.vue 相同逻辑）
      const accountRes = await AccountService.queryAccounts({
        partyId: item.partyId,
        hasTradeAccount: true,
      });

      const accountInfo =
        accountRes.data.find(
          (x: any) =>
            x.tradeAccount &&
            x.tradeAccount.accountNumber == item.source?.displayNumber
        ) ?? {};

      const bankInfo = withdrawalInfo.Reference?.Request || {};

      // 获取用户交易账户列表
      const userTradeAccounts = accountRes.data.map((x: any) => x.tradeAccount);

      const detailItem = {
        id: item.id,
        paymentId: item.payment.id,
        selected: false,
        // 提现信息
        createdOn: item.createdOn,
        status: item.payment.status,
        refId: item.payment.id,
        userName:
          item.displayName || `${item.user?.firstName} ${item.user?.lastName}`,
        nativeName: item.user?.nativeName || "",
        email: item.user?.email || "",
        accountNumber: item.source?.displayNumber || "",
        salesCode: accountInfo?.code || "",
        ibGroup: accountInfo?.group || "",
        currencyId: item.currencyId,
        balanceInCents: item.source?.balanceInCents || 0,
        accountType: item.source?.accountType,
        sourceDisplayNumber: item.source?.displayNumber || "",
        amount: item.amount,
        fromCurrencyId: item.currencyId,
        toCurrencyId: withdrawalInfo.TargetCurrencyId,
        exchangeRate: withdrawalInfo.ExchangeRate,
        actualAmount: item.amount * withdrawalInfo.ExchangeRate,
        // 支付方式
        paymentMethod: item.payment?.paymentServiceName || "",
        countryOfBank: (phonesData as any)[bankInfo.bankCountry]?.name || "",
        bankCountryCode: bankInfo.bankCountry || "",
        bankName: bankInfo.bankName || "",
        branchName: bankInfo.branchName || "",
        city: bankInfo.city || "",
        state: bankInfo.state || "",
        accountHolder: bankInfo.holder || "",
        bankAccountNo: bankInfo.accountNo || "",
        confirmAccountNo: bankInfo.confirmAccountNo || "",
        bsb: bankInfo.bsb || "",
        swiftCode: bankInfo.swiftCode || "",
        walletAddress: bankInfo.walletAddress || "",
        // 用于复制的额外数据
        userTradeAccounts: userTradeAccounts,
        accountInfo: accountInfo,
      };

      detailData.value.push(detailItem);
    }

    // 更新选择状态
    updateSelection();
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

// 处理全选
const handleSelectAll = (val: boolean) => {
  detailData.value.forEach((item: any) => {
    item.selected = val;
  });
  updateSelection();
};

// 处理单行选择变化
const handleSelectionChange = () => {
  updateSelection();
};

// 更新选择状态
const updateSelection = () => {
  const selectedItems = detailData.value.filter((item: any) => item.selected);
  selectedIds.value = selectedItems.map((item: any) => item.paymentId);

  const totalCount = detailData.value.length;
  const selectedCount = selectedItems.length;

  isAllSelected.value = totalCount > 0 && selectedCount === totalCount;
  isIndeterminate.value = selectedCount > 0 && selectedCount < totalCount;
};

// 执行批量处理
const executeBatch = () => {
  if (selectedIds.value.length === 0) return;

  openConfirmBox(async () => {
    isExecuting.value = true;
    let successCount = 0;
    let failCount = 0;

    try {
      for (const paymentId of selectedIds.value) {
        try {
          await PaymentService.executePaymentById(paymentId);
          successCount++;
        } catch (error) {
          failCount++;
          console.error(`Failed to execute payment ${paymentId}:`, error);
        }
      }

      if (failCount === 0) {
        MsgPrompt.success(`成功处理 ${successCount} 条记录`);
      } else {
        MsgPrompt.warning(`成功: ${successCount} 条, 失败: ${failCount} 条`);
      }

      visible.value = false;
      emits("success");
    } catch (error) {
      MsgPrompt.error(error);
    } finally {
      isExecuting.value = false;
    }
  });
};

// 复制单行信息（格式与 WithdrawInfo.vue 一致）
const copyItemInfo = (item: any) => {
  const formatAmount = (balance: number, currencyId: number) => {
    return filters.toCurrency(balance, currencyId, language);
  };

  const getCurrencyName = (currencyId: number) => {
    return t(`type.currency.${currencyId}`);
  };

  const getStatusText = (status: number) => {
    return t(`type.paymentStatus.${status}`);
  };

  const getBalanceLabel = () => {
    if (item.accountType === TransactionAccountTypes.Wallet) {
      return t("fields.walletBalance");
    } else if (item.accountType === TransactionAccountTypes.TradeAccount) {
      return t("fields.accountBalance");
    }
    return t("fields.balance");
  };

  // 生成用户账户表格行
  const userAccountRows = (item.userTradeAccounts || [])
    .filter((acc: any) => acc)
    .map(
      (acc: any) => `
      <tr>
        <td>${acc.accountNumber || ""}</td>
        <td>${formatAmount(acc.balanceInCents || 0, acc.currencyId)}</td>
      </tr>
    `
    )
    .join("");

  // 生成账户信息行（仅当是 TradeAccount 时显示）
  const accountInfoRows =
    item.accountType === TransactionAccountTypes.TradeAccount
      ? `
      <tr>
        <td>${t("fields.accountNumber")}</td>
        <td>${item.sourceDisplayNumber}</td>
      </tr>
      <tr>
        <td>${t("fields.salesCode")}</td>
        <td>${item.salesCode}</td>
      </tr>
      <tr>
        <td>${t("fields.ibGroup")}</td>
        <td>${item.ibGroup}</td>
      </tr>
    `
      : "";

  const htmlContent = `
    <div>
      Dear Tony: <br />
      This is ${item.userName}(${item.nativeName})'s
      Withdrawal request form, the system has been updated. Please find the
      attached file. <br />
      <b>Withdrawal Amount (Platform): ${formatAmount(
        item.amount,
        item.fromCurrencyId
      )}<br />
      Rate: ${item.exchangeRate}<br />
      <span style="color: red">Withdrawal Amount (Actual): ${formatAmount(
        item.actualAmount,
        item.toCurrencyId
      )}</span>
      </b>
      <br /><br />
      Please check it. Thanks.
      <br /><br />
      Regards,<br />
      ${user.name}
      <br /><br />
    </div>
    <table style="font-family: arial, sans-serif; border-collapse: collapse; width: 100%; border: 1px solid #808080;">
      <tr>
        <td colspan="2" style="text-align: center; font-weight: bold; border: 1px solid #808080; padding: 8px;">
          ${t("title.withdrawInfo")}
        </td>
      </tr>
      <tr>
        <td style="width: 50%; border: 1px solid #808080; padding: 8px;">${t(
          "fields.createdOn"
        )}</td>
        <td style="width: 50%; border: 1px solid #808080; padding: 8px;">${
          item.createdOn
        }</td>
      </tr>
      <tr>
        <td style="border: 1px solid #808080; padding: 8px;">${t(
          "fields.status"
        )}</td>
        <td style="border: 1px solid #808080; padding: 8px;">${getStatusText(
          item.status
        )}</td>
      </tr>
      <tr>
        <td style="border: 1px solid #808080; padding: 8px;">${t(
          "fields.refId"
        )}</td>
        <td style="border: 1px solid #808080; padding: 8px;">${item.refId}</td>
      </tr>
      <tr>
        <td style="border: 1px solid #808080; padding: 8px;">${t(
          "fields.nativeName"
        )}</td>
        <td style="border: 1px solid #808080; padding: 8px;">${
          item.nativeName
        }</td>
      </tr>
      <tr>
        <td style="border: 1px solid #808080; padding: 8px;">${t(
          "fields.email"
        )}</td>
        <td style="border: 1px solid #808080; padding: 8px;">${item.email}</td>
      </tr>
      ${accountInfoRows}
      <tr>
        <td style="border: 1px solid #808080; padding: 8px;">${getBalanceLabel()}</td>
        <td style="border: 1px solid #808080; padding: 8px;">${formatAmount(
          item.balanceInCents,
          item.currencyId
        )} ( ${getCurrencyName(item.currencyId)} )</td>
      </tr>
      <tr>
        <td style="border: 1px solid #808080; padding: 8px;">${t(
          "fields.withdrawAmount"
        )}</td>
        <td style="border: 1px solid #808080; padding: 8px;">${formatAmount(
          item.amount,
          item.fromCurrencyId
        )} ( ${getCurrencyName(item.fromCurrencyId)} )</td>
      </tr>
      <tr>
        <td style="border: 1px solid #808080; padding: 8px;">${t(
          "fields.exchangeRate"
        )}</td>
        <td style="border: 1px solid #808080; padding: 8px;">${
          item.exchangeRate
        }</td>
      </tr>
      <tr>
        <td style="border: 1px solid #808080; padding: 8px;">${t(
          "fields.actualAmount"
        )}</td>
        <td style="border: 1px solid #808080; padding: 8px;">${formatAmount(
          item.actualAmount,
          item.toCurrencyId
        )} ( ${getCurrencyName(item.toCurrencyId)} )</td>
      </tr>
    </table>
    <div style="margin: 10px 0"></div>
    <table style="font-family: arial, sans-serif; border-collapse: collapse; width: 100%; border: 1px solid #808080;">
      <tr>
        <td colspan="2" style="text-align: center; font-weight: bold; border: 1px solid #808080; padding: 8px;">
          ${t("fields.paymentMethod")}
        </td>
      </tr>
      <tr>
        <td style="width: 50%; border: 1px solid #808080; padding: 8px;">${t(
          "fields.method"
        )}</td>
        <td style="width: 50%; border: 1px solid #808080; padding: 8px;">${
          item.paymentMethod
        }</td>
      </tr>
      <tr>
        <td style="border: 1px solid #808080; padding: 8px;">${t(
          "fields.countryOfBank"
        )}</td>
        <td style="border: 1px solid #808080; padding: 8px;">${
          item.countryOfBank
        }</td>
      </tr>
      <tr>
        <td style="border: 1px solid #808080; padding: 8px;">${t(
          "fields.bankName"
        )}</td>
        <td style="border: 1px solid #808080; padding: 8px;">${
          item.bankName
        }</td>
      </tr>
      <tr>
        <td style="border: 1px solid #808080; padding: 8px;">${t(
          "fields.branchName"
        )}</td>
        <td style="border: 1px solid #808080; padding: 8px;">${
          item.branchName
        }</td>
      </tr>
      <tr>
        <td style="border: 1px solid #808080; padding: 8px;">${t(
          "fields.city"
        )}</td>
        <td style="border: 1px solid #808080; padding: 8px;">${item.city}</td>
      </tr>
      <tr>
        <td style="border: 1px solid #808080; padding: 8px;">${t(
          "fields.state"
        )}</td>
        <td style="border: 1px solid #808080; padding: 8px;">${item.state}</td>
      </tr>
      <tr>
        <td style="border: 1px solid #808080; padding: 8px;">${t(
          "fields.accountHolder"
        )}</td>
        <td style="border: 1px solid #808080; padding: 8px;">${
          item.accountHolder
        }</td>
      </tr>
      <tr>
        <td style="border: 1px solid #808080; padding: 8px;">${t(
          "fields.accountNo"
        )}</td>
        <td style="border: 1px solid #808080; padding: 8px;">${
          item.bankAccountNo
        }</td>
      </tr>
      <tr>
        <td style="border: 1px solid #808080; padding: 8px;">${t(
          "fields.confirmAccountNo"
        )}</td>
        <td style="border: 1px solid #808080; padding: 8px;">${
          item.confirmAccountNo
        }</td>
      </tr>
      <tr>
        <td style="border: 1px solid #808080; padding: 8px;">${t(
          "fields.bsb"
        )}</td>
        <td style="border: 1px solid #808080; padding: 8px;">${item.bsb}</td>
      </tr>
      <tr>
        <td style="border: 1px solid #808080; padding: 8px;">${t(
          "fields.swiftCode"
        )}</td>
        <td style="border: 1px solid #808080; padding: 8px;">${
          item.swiftCode
        }</td>
      </tr>
      <tr>
        <td style="border: 1px solid #808080; padding: 8px;">${t(
          "fields.usdtWalletAddress"
        )}</td>
        <td style="border: 1px solid #808080; padding: 8px;">${
          item.walletAddress
        }</td>
      </tr>
    </table>
    <div style="margin: 10px 0"></div>
    <table style="font-family: arial, sans-serif; border-collapse: collapse; width: 100%; border: 1px solid #808080;">
      <tr>
        <td colspan="2" style="text-align: center; font-weight: bold; border: 1px solid #808080; padding: 8px;">
          ${t("title.userAccounts")}
        </td>
      </tr>
      <tr>
        <td style="text-align: center; font-weight: bold; border: 1px solid #808080; padding: 8px;">
          ${t("fields.accountNo")}
        </td>
        <td style="text-align: center; font-weight: bold; border: 1px solid #808080; padding: 8px;">
          ${t("fields.balance")}
        </td>
      </tr>
      ${userAccountRows}
    </table>
  `;

  // 创建临时元素并复制
  const tempDiv = document.createElement("div");
  tempDiv.innerHTML = htmlContent;
  document.body.appendChild(tempDiv);

  try {
    Clipboard.copy(tempDiv);
    MsgPrompt.success("Copy success");
  } catch (error) {
    MsgPrompt.error("Copy failed");
  } finally {
    document.body.removeChild(tempDiv);
  }
};

// 显示二维码 Modal
const showQrCodeModal = async (item: any) => {
  currentWalletAddress.value = item.walletAddress;
  qrCodeVisible.value = true;
  await nextTick();

  // 清除旧的二维码
  if (qrCodeRef.value) {
    while (qrCodeRef.value.firstChild) {
      qrCodeRef.value.removeChild(qrCodeRef.value.firstChild);
    }
    // 生成新的二维码
    new QRCode(qrCodeRef.value, {
      text: item.walletAddress,
      width: 200,
      height: 200,
      colorDark: "#000000",
      colorLight: "#ffffff",
      correctLevel: QRCode.CorrectLevel.H,
    });
  }
};

defineExpose({
  show,
});
</script>

<style scoped>
.table-responsive {
  overflow-x: auto !important;
  overflow-y: auto !important;
}
.table {
  min-width: 100%;
  width: max-content;
  border-collapse: separate;
  border-spacing: 0;
}
.table td,
.table th {
  white-space: nowrap;
  padding: 8px 12px;
}
.sticky-col {
  position: sticky;
  left: 0;
  background-color: #fff;
  z-index: 1;
  box-shadow: 2px 0 5px -2px rgba(0, 0, 0, 0.1);
}
thead .sticky-col {
  background-color: #f8f9fa;
  z-index: 2;
}
tbody tr:nth-child(even) .sticky-col {
  background-color: #f8f9fa;
}
tbody tr:hover .sticky-col {
  background-color: #f5f5f5;
}
/* 高亮列样式 */
.highlight-col {
  background-color: #fffde0 !important;
}
thead .highlight-col {
  background-color: #fff3cd !important;
  font-weight: bold;
}
/* 右侧固定列样式 */
.sticky-col-right {
  position: sticky;
  right: 0;
  background-color: #fff;
  z-index: 1;
  box-shadow: -2px 0 5px -2px rgba(0, 0, 0, 0.1);
}
thead .sticky-col-right {
  background-color: #f8f9fa;
  z-index: 2;
}
tbody tr:nth-child(even) .sticky-col-right {
  background-color: #f8f9fa;
}
tbody tr:hover .sticky-col-right {
  background-color: #f5f5f5;
}
/* 二维码样式 */
.qrcode {
  display: inline-block;
  padding: 10px;
  background-color: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}
.qrcode img {
  display: block;
}
</style>
