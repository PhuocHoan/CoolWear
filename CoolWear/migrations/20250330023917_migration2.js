/**
 * @param { import("knex").Knex } knex
 * @returns { Promise<void> }
 */
exports.up = async function (knex) {
  // Start a transaction to ensure all schema changes are atomic
  await knex.transaction(async (trx) => {
    // 1. Create the product_variant table
    if (!(await trx.schema.hasTable('product_variant'))) {
      await trx.schema.createTable('product_variant', function (table) {
        table
          .increments('variant_id')
          .primary()
          .comment('Mã biến thể sản phẩm, khóa chính, tự động tăng');
        table
          .integer('product_id')
          .notNullable()
          .comment('Mã sản phẩm, khóa ngoại');
        table
          .integer('color_id')
          .notNullable()
          .comment('Mã màu sắc, khóa ngoại');
        table
          .integer('size_id')
          .notNullable()
          .comment('Mã kích thước, khóa ngoại');
        table
          .integer('stock_quantity')
          .notNullable()
          .defaultTo(0)
          .comment('Số lượng tồn kho');

        // Add foreign key constraints
        table.foreign('product_id').references('product_id').inTable('product');
        table
          .foreign('color_id')
          .references('color_id')
          .inTable('product_color');
        table.foreign('size_id').references('size_id').inTable('product_size');

        // Add unique constraint
        table.unique(['product_id', 'color_id', 'size_id']);
      });

      // Add comment for the table
      await trx.raw(
        'COMMENT ON TABLE "product_variant" IS \'Bảng biến thể sản phẩm\'',
      );
    }

    // 2. Update order table structure
    await trx.schema.alterTable('order', async function (table) {
      // Add status field if it doesn't exist
      if (!(await trx.schema.hasColumn('order', 'status'))) {
        table
          .string('status', 20)
          .notNullable()
          .defaultTo('Đang xử lý')
          .comment('Trạng thái đơn hàng, mặc định là "Đang xử lý"');
      }

      // Make customer_id nullable if not already
      table.integer('customer_id').nullable().alter();

      // Add point_used field if it doesn't exist
      if (!(await trx.schema.hasColumn('order', 'point_used'))) {
        table
          .integer('point_used')
          .notNullable()
          .defaultTo(0)
          .comment('Số điểm sử dụng, mặc định là 0');
      }
    });

    // 3. Update order_item to reference product_variant instead of product directly
    if (await trx.schema.hasTable('order_item')) {
      await trx.schema.alterTable('order_item', async function (table) {
        // Add variant_id column if it doesn't exist
        if (!(await trx.schema.hasColumn('order_item', 'variant_id'))) {
          table
            .integer('variant_id')
            .nullable()
            .comment('Mã biến thể sản phẩm, khóa ngoại');

          // Add foreign key to product_variant
          table
            .foreign('variant_id')
            .references('variant_id')
            .inTable('product_variant');
        }
      });
    }

    // 4. Remove stock_quantity from product table if it exists
    if (await trx.schema.hasColumn('product', 'stock_quantity')) {
      await trx.schema.alterTable('product', function (table) {
        table.dropColumn('stock_quantity');
      });
    }
  });
};

/**
 * @param { import("knex").Knex } knex
 * @returns { Promise<void> }
 */
exports.down = async function (knex) {
  // Start a transaction to ensure all schema changes are atomic
  await knex.transaction(async (trx) => {
    // 1. Add back stock_quantity to products table
    await trx.schema.alterTable('product', function (table) {
      table
        .integer('stock_quantity')
        .notNullable()
        .defaultTo(0)
        .comment('Số lượng tồn kho');
    });

    // 2. Remove variant_id from order_item
    if (
      (await trx.schema.hasTable('order_item')) &&
      (await trx.schema.hasColumn('order_item', 'variant_id'))
    ) {
      await trx.schema.alterTable('order_item', function (table) {
        table.dropForeign('variant_id');
        table.dropColumn('variant_id');
      });
    }

    // 3. Restore order table to previous state
    await trx.schema.alterTable('order', async function (table) {
      // Make customer_id required again
      table.integer('customer_id').notNullable().alter();

      // Drop point_used if added
      if (await trx.schema.hasColumn('order', 'point_used')) {
        table.dropColumn('point_used');
      }

      // Drop status field if added
      if (await trx.schema.hasColumn('order', 'status')) {
        table.dropColumn('status');
      }
    });

    // 4. Drop product_variant table
    if (await trx.schema.hasTable('product_variant')) {
      await trx.schema.dropTable('product_variant');
    }
  });
};
